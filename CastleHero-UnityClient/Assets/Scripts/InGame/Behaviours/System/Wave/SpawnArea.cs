using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.System.MonsterFactory;
using RGLabs.InGame.System.Spawn;
using RGLabs.InGame.System.Wave.Data;
using RGLabs.InGame.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.Behaviours.System.Wave
{
    public class SpawnArea : MonoBehaviour
    {
        private struct EntitySpace
        {
            public int index;
            public float size;
        }

        private struct CreationRequest
        {
            public Vector2 position;
            public UnitEntity entity;
        }

        [SerializeField] private int _id;
        [SerializeField] private float _size;
        [SerializeField] private Shared _shared;

        private Queue<CreationRequest[]> _queue;

        private IUnitFactory _factory;
        private Dictionary<int, EntitySpace[]> _spawnSpaceMap;

        private void Awake()
        {
            _shared.spawnDataStream.Collect += OnCollectSpawnEvent;
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

        private void OnCollectSpawnEvent(SpawnEventData data)
        {
            if (_id != data.area)
                return;

            var spaces = ConstructSpaces(data.entities, out float totalSize);
            float leftSpace = _size - totalSize;
            if (leftSpace > 0)
                AddRandomSpace(ref spaces, leftSpace);

            spaces.Shuffle();
            //Array.Sort(spaces, (_, _) => Random.Range(-1, 2));

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