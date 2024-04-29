using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Common;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Utility;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.System.Wave
{
    public class CreationHelper
    {
        private struct EntitySpace
        {
            public int index;
            public Vector2 size;
        }

        private readonly int _bufferSize;
        private readonly int _areaId;
        private readonly float _size;
        private readonly Vector2 _offset;

        private readonly Vector2 _cornerA;
        private readonly Vector2 _cornerB;

        private readonly UnitDB _db;
        private readonly Queue<SpawnEvent> _queue;

        private readonly UnitEntity[] _entityBuffer;
        private readonly EntitySpace[] _spaceBuffer;
        private readonly UnitCreation[] _creationBuffer;

        public CreationHelper(int areaId, UnitDB db, Vector2 cornerA, Vector2 cornerB)
        {
            _areaId = areaId;
            _cornerA = cornerA;
            _cornerB = cornerB;
            _bufferSize = Constants.SpawnBufferSize;
            _db = db;

            _queue = new();
            _entityBuffer = new UnitEntity[_bufferSize];
            _spaceBuffer = new EntitySpace[_bufferSize];
            _creationBuffer = new UnitCreation[_bufferSize];

            MessageBroker.Default.Receive<SpawnEvent[]>().Subscribe(OnReceiveSpawnEvents);
        }
        
        public void SetUpBuffers(Action<UnitCreation> onResult)
        {
            if (_queue.Count == 0)
                return;
            
            int bufferLength = SetUpEntityBuffer();
            SetUpSpaceBuffer(bufferLength, out var leftSpace);
            
            bufferLength = ApplyBlank(bufferLength, leftSpace);
            bufferLength = SetUpCreationBuffer(bufferLength);
            
            for (int i = 0; i < bufferLength; ++i)
            {
                onResult.Invoke(_creationBuffer[i]);
            }
        }

        private void OnReceiveSpawnEvents(SpawnEvent[] data)
        {
            foreach (var ev in data)
            {
                if(ev.area == _areaId)
                    _queue.Enqueue(ev);
            }
        }

        /// <summary>
        /// 유닛 데이터 버퍼 할당
        /// </summary>
        /// <returns>버퍼의 유효 길이</returns>
        private int SetUpEntityBuffer()
        {
            int count = Mathf.Min(_queue.Count, _bufferSize);
            int left = count;
            int i = 0;
            while (left > 0)
            {
                var data = _queue.Dequeue();
                if (!_db.TryFind(data.id, out var entity))
                    --count;
                else
                    _entityBuffer[i++] = entity;
                
                --left;
            }

            return count;
        }
        
        /// <summary>
        /// 공간 버퍼 할당
        /// </summary>
        /// <param name="count">할당할 길이</param>
        /// <param name="leftSpace">할당 후 남은 공간</param>
        private void SetUpSpaceBuffer(int count, out Vector2 leftSpace)
        {
            leftSpace = _cornerB - _cornerA;
            
            var direction = leftSpace.normalized;
            for (int i = 0; i < count; ++i)
            {
                var entity = _entityBuffer[i];
                var size = new Vector2(entity.size, entity.size) * direction;
                _spaceBuffer[i].index = i;
                _spaceBuffer[i].size = size;

                leftSpace -= size;
            }
        }
        
        /// <summary>
        /// 빈 공간을 랜덤하게 적용
        /// </summary>
        /// <param name="startIndex">적용 시작 인덱스</param>
        /// <param name="availableSpace">사용 가능 공간</param>
        /// <returns>적용된 빈 공간 개수를 더한 길이</returns>
        private int ApplyBlank(int startIndex, Vector2 availableSpace)
        {
            int leftSlot = _bufferSize - startIndex;
            if (leftSlot <= 0)
                return startIndex;

            int random = Mathf.Min(Random.Range(1, leftSlot), 5);
            int end = startIndex + random;
            var blankSpace = availableSpace / random;
            for (int i = startIndex; i < end; ++i)
            {
                _spaceBuffer[i].index = -1;
                _spaceBuffer[i].size = blankSpace;
            }

            _spaceBuffer.Shuffle(end);
            return end;
        }

        /// <summary>
        /// 생성 버퍼 할당
        /// </summary>
        /// <param name="count">빈 공간을 포함한 개수</param>
        /// <returns>버퍼의 유효 길이</returns>
        private int SetUpCreationBuffer(int count)
        {
            Vector2 lastPosition = _cornerA;
            Vector2 lastHalfSize = default;

            int sBufferIndex = 0;
            int cBufferIndex = 0;
            for (; sBufferIndex < count; ++sBufferIndex)
            {
                var space = _spaceBuffer[sBufferIndex];
                var halfSize = space.size * 0.5f;
                var position = lastPosition + lastHalfSize + halfSize;
                var entityIndex = space.index;
                if (entityIndex != -1)
                {
                    _creationBuffer[cBufferIndex].entity = _entityBuffer[entityIndex];
                    _creationBuffer[cBufferIndex].position = position;
                    ++cBufferIndex;
                }

                lastPosition = position;
                lastHalfSize = halfSize;
            }

            return cBufferIndex;
        }
    }
}