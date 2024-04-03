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

        private IUnitFactory _factory;
        private Dictionary<int, EntitySpace[]> _spawnSpaceMap;

        public void Init(DataStream<SpawnEvent> stream)
        {
            stream.Collect += OnCollectSpawnEvent;
        }
        
        private void Awake()
        {
            _factory = new DefaultUnitFactory();
            _queue = new();
        }

        private void Update()
        {
            while (_queue.Count > 0)
            {
                var requests = _queue.Dequeue();
                foreach (var request in requests)
                {
                    _factory.PushCreationRequest<MonsterGameUnit>(request.entity,
                        (unit) => { unit.transform.position = request.position; });
                }
            }
        }

        private void OnCollectSpawnEvent(SpawnEvent data)
        {
            if (_id != data.area)
                return;

            var spaces = ConstructSpaces(data.entities, out float totalSize);
            float leftSpace = _size - totalSize;
            if (leftSpace > 0)
                AddRandomSpace(ref spaces, leftSpace);

            spaces.Shuffle();

            RegisterCreationRequests(data.spawnAt, spaces, data.entities);
        }

        private void RegisterCreationRequests(SpawnAt spawnAt, EntitySpace[] spaces, UnitEntity[] entities)
        {
            var position = transform.position;
            float lastX = position.x - (_size * 0.5f);
            float lastHalfSize = 0f;
            float y = position.y;

            int requestIndex = 0;
            var requests = new CreationRequest[entities.Length];
            foreach (var space in spaces)
            {
                float halfSize = space.size * 0.5f;
                float x = lastX + lastHalfSize + halfSize;
                if (space.index != -1)
                {
                    requests[requestIndex++] = new CreationRequest
                    {
                        position = new Vector2(x, y),
                        entity = entities[space.index]
                    };
                }

                lastX = x;
                lastHalfSize = halfSize;
            }

            switch (spawnAt)
            {
                case SpawnAt.ForEach:
                    foreach (var req in requests)
                    {
                        _queue.Enqueue(new[] { req });
                    }

                    break;
                case SpawnAt.AtOnce:
                    _queue.Enqueue(requests);
                    break;
            }
        }

        private EntitySpace[] ConstructSpaces(UnitEntity[] entities, out float totalSize)
        {
            totalSize = 0f;
            int count = entities.Length;
            var spaces = new EntitySpace[count];
            for (int i = 0; i < count; ++i)
            {
                float size = entities[i].size;
                spaces[i] = new EntitySpace
                {
                    index = i,
                    size = size
                };
                totalSize += size;
            }

            return spaces;
        }

        private void AddRandomSpace(ref EntitySpace[] spaces, float availableSpace)
        {
            if (availableSpace < 0 || Mathf.Approximately(availableSpace, 0))
                return;

            int random = Random.Range(1, 5);
            int originLength = spaces.Length;
            int newLength = originLength + random;
            Array.Resize(ref spaces, newLength);

            float blankSpace = availableSpace / random;
            for (int i = originLength; i < newLength; ++i)
            {
                spaces[i] = new EntitySpace
                {
                    index = -1,
                    size = blankSpace
                };
            }
        }
    }
}