using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.Data.DB;
using RGLabs.Data.Model;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.System.Wave.Creation
{
    public class CreationHelper : ICreationHelper
    {
        private struct Unit
        {
            public int lv;
            public int id;
        }
        
        private struct UnitSpace
        {
            public bool valid;
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

        private readonly Unit[] _unitBuffer;
        private readonly UnitSpace[] _spaceBuffer;
        private readonly UnitCreation[] _creationBuffer;

        public CreationHelper(int areaId, UnitDB db, Vector2 cornerA, Vector2 cornerB)
        {
            _areaId = areaId;
            _cornerA = cornerA;
            _cornerB = cornerB;
            _bufferSize = Constants.BufferSize;
            _db = db;

            _queue = new();
            _unitBuffer = new Unit[_bufferSize];
            _spaceBuffer = new UnitSpace[_bufferSize];
            _creationBuffer = new UnitCreation[_bufferSize];

            MessageBroker.Default.Receive<SpawnEvent[]>().Subscribe(OnReceiveSpawnEvents);
        }

        public void SetUpCreations(Action<UnitCreation> onCreation)
        {
            if (_queue.Count == 0)
                return;

            int bufferLength = SetUpUnitBuffer();
            SetUpSpaceBuffer(bufferLength, out var leftSpace);
            bufferLength = ApplyBlank(bufferLength, leftSpace);
            bufferLength = SetUpCreationBuffer(bufferLength);
            for (int i = 0; i < bufferLength; ++i)
            {
                onCreation.Invoke(_creationBuffer[i]);
            }
        }

        private void OnReceiveSpawnEvents(SpawnEvent[] data)
        {
            foreach (var ev in data)
            {
                if (ev.area == _areaId)
                    _queue.Enqueue(ev);
            }
        }

        /// <summary>
        /// 유닛 데이터 버퍼 할당
        /// </summary>
        /// <returns>버퍼의 유효 길이</returns>
        private int SetUpUnitBuffer()
        {
            int count = Mathf.Min(_queue.Count, _bufferSize);
            int i = 0;
            while (_queue.Count > 0)
            {
                var data = _queue.Dequeue();
                _unitBuffer[i++] = new Unit
                {
                    lv = data.lv,
                    id = data.id
                };
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
            int bufferLength = _spaceBuffer.Length;
            for (int i = 0; i < bufferLength; ++i)
            {
                if (i < count)
                {
                    var unit = _unitBuffer[i];
                    var diameter = _db.sizeCache.GetValueOrDefault(unit.id, 0.3f) * 2f;
                    var size = new Vector2(diameter, diameter) * direction;
                    _spaceBuffer[i].index = i;
                    _spaceBuffer[i].size = size;
                    _spaceBuffer[i].valid = true;

                    leftSpace -= size;
                }
                else
                {
                    _spaceBuffer[i].valid = false;
                }
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

            int random = Mathf.Min(2, 4);
            int end = startIndex + random;
            var blankSpace = availableSpace / random;
            for (int i = startIndex; i < end; ++i)
            {
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
                if (space.valid)
                {
                    var unit = _unitBuffer[space.index];
                    _creationBuffer[cBufferIndex].id = unit.id;
                    _creationBuffer[cBufferIndex].lv = unit.lv;
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