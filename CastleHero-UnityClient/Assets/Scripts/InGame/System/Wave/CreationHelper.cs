using System;
using RGLabs.Common;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.System.Wave
{
    public class CreationHelper
    {
        private struct EntitySpace
        {
            public int index;
            public float size;
        }

        private readonly int _areaId;
        private readonly float _size;
        private readonly Vector2 _offset;

        private readonly DataStream<CreationEvent> _output;

        public CreationHelper(int areaId, float size, Vector2 offset, DataStream<SpawnEvent> input,
            DataStream<CreationEvent> output)
        {
            _areaId = areaId;
            _size = size;
            _offset = offset;
            _output = output;

            input.Collect += OnCollectData;
        }

        private void OnCollectData(SpawnEvent data)
        {
            if (_areaId != data.area)
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
            float lastX = _offset.x - (_size * 0.5f);
            float lastHalfSize = 0f;
            float y = _offset.y;

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
                        var data = new CreationEvent
                        {
                            requests = new[] { req }
                        };

                        _output.Emit(data);
                    }

                    break;
                case SpawnAt.AtOnce:
                    _output.Emit(new CreationEvent { requests = requests });
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