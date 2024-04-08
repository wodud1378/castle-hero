using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.System.Wave;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Wave
{
    public class SpawnArea : MonoBehaviour
    {
        [SerializeField] private int _id;
        [SerializeField] private float _size;

        private CreationHelper _creationHelper;
        private DataStream<UnitBehaviour> _releaseStream;
        private IUnitFactory _factory;

        public void Init(DataStream<SpawnEvent> spawnEventStream, DataStream<CreationRequest[]> creationStream, DataStream<UnitBehaviour> releaseStream)
        {
            _factory = new DefaultUnitFactory(InGameContext.Pools);
            _creationHelper = new CreationHelper(_id, _size, transform.position, spawnEventStream, creationStream);
            _releaseStream = releaseStream;

            creationStream.Collect += OnCollectCreationRequest;
        }

        private void OnCollectCreationRequest(CreationRequest[] requests)
        {
            foreach (var request in requests)
            {
                Create(request);
            }
        }

        private async void Create(CreationRequest request)
        {
            var position = request.position;
            var unit = await _factory.Create<UnitBehaviour>(request.entity, position);
            unit.defaultDestination = CalculateTargetPosition(position);
            unit.autoRelease = false;
            unit.OnDead += OnUnitDead;
        }

        private Vector2 CalculateTargetPosition(Vector2 from)
        {
            //Temp
            float size = 1.34f;
            var to = Vector2.zero;
            var direction = (to - from).normalized;
            return to - (direction * size);
        }

        private void OnUnitDead(UnitBehaviour unit)
        {
            _releaseStream.Emit(unit);

            unit.OnDead -= OnUnitDead;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(_size, 1f, 1f));
        }
    }
}