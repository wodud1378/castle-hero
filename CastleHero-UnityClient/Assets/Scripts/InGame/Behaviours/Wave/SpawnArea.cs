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
        private IUnitFactory _factory;

        public void Init(DataStream<SpawnEvent> spawnEventStream, DataStream<CreationRequest[]> creationStream)
        {
            _factory = new DefaultUnitFactory();
            _creationHelper = new CreationHelper(_id, _size, transform.position, spawnEventStream, creationStream);
            
            creationStream.Collect += OnCollectCreationRequest;
        }

        private void OnCollectCreationRequest(CreationRequest[] requests)
        {
            foreach (var request in requests)
            {
                _factory.PushCreationRequest<MonsterGameUnit>(request.entity,
                    (unit) => { unit.transform.position = request.position; });
            }
        }
    }
}