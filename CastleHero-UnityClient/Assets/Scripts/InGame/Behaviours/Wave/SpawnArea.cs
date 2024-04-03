using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.System.Wave;
using RGLabs.InGame.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.Behaviours.Wave
{
    public class SpawnArea : MonoBehaviour
    {
        private struct EntitySpace
        {
            public int index;
            public float size;
        }

        [SerializeField] private int _id;
        [SerializeField] private float _size;

        private Queue<CreationRequest[]> _queue;

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

        private void Awake()
        {
            _queue = new();
        }
    }
}