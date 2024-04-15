using System;
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
        private DataStream<ReleaseEvent> _releaseStream;
        private IUnitFactory _factory;

        public void Init(DataStream<SpawnEvent> spawnEventStream, DataStream<CreationEvent> creationStream, DataStream<ReleaseEvent> releaseStream)
        {
            _factory = new DefaultUnitFactory(InGameContext.pools);
            _creationHelper = new CreationHelper(_id, _size, transform.position, spawnEventStream, creationStream);
            _releaseStream = releaseStream;

            creationStream.Collect += OnCollectData;
        }

        private void OnCollectData(CreationEvent data)
        {
            foreach (var request in data.requests)
            {
                Create(request);
            }
        }

        private async void Create(CreationRequest request)
        {
            var position = request.position;
            var unit = await _factory.Create<UnitBehaviour>(request.entity, position);
            unit.defaultDestination = unit.ClosestPoint(position, InGameContext.camp.castle);
            unit.autoRelease = false;
            unit.OnDead += OnUnitDead;
        }

        private void OnUnitDead(UnitBehaviour unit)
        {
            _releaseStream.Emit(new ReleaseEvent
            {
                unit = unit
            });

            unit.OnDead -= OnUnitDead;
        }

        private void OnDestroy()
        {
            _creationHelper = null;
            _factory = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(_size, 1f, 1f));
        }
    }
}