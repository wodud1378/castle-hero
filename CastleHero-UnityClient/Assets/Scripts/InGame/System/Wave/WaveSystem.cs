using System;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.System.Wave.Data;
using UnityEngine;

namespace RGLabs.InGame.System.Wave
{
    [Serializable]
    public class WaveSystem : ISystem
    {
        [SerializeField] private Shared _shared;
        [SerializeField] private WaveData[] _waves;
        
        private MonsterDB _db;
        private DataStream<SpawnEventData> _stream;

        private int _cursor;
        private float _timeSinceActive;

        private float _currentTime;
        private float _timeStep;

        private bool _activated;

        /// <summary>
        /// 웨이브 인덱스 커서의 유효 여부
        /// </summary>
        private bool IsCursorValid => _cursor >= 0 && _cursor < _waves.Length;

        /// <summary>
        /// 생성 가능 시점 여부 반환 (진행중인 웨이브의 업데이트 주기)
        /// </summary>
        private bool OnStep => _currentTime >= _timeStep;

        /// <summary>
        /// 현재 웨이브 진행 여부
        /// </summary>
        private bool OnWave => _timeSinceActive >= _waves[_cursor].start && _timeSinceActive < _waves[_cursor].end;
        
        public void Init()
        {
            _db = _shared.monsterDB;
            _stream = _shared.spawnDataStream;

            _cursor = 0;
            _timeSinceActive = 0f;

            _currentTime = 0f;
            _timeStep = 0f;

            _activated = false;
        }

        public void ProcessUpdate(float deltaTime)
        {
            if (!IsCursorValid)
                return;

            _timeSinceActive += deltaTime;
            _currentTime += deltaTime;

            UpdateCursor();
            ProcessSpawn();
        }
        
        /// <summary>
        /// 웨이브 지정 커서 업데이트
        /// </summary>
        private void UpdateCursor()
        {
            if (_timeSinceActive >= _waves[_cursor].end)
            {
                ++_cursor;
                _activated = false;
            }

            if (!IsCursorValid)
                return;

            if (!_activated)
            {
                OnWaveStart();
                _activated = true;
            }
        }

        /// <summary>
        /// 웨이브 시작 시 업데이트 주기 설정
        /// </summary>
        private void OnWaveStart()
        {
            _currentTime = 0f;
            _timeStep = _waves[_cursor].timeStep;
        }

        /// <summary>
        /// 생성 처리
        /// </summary>
        private void ProcessSpawn()
        {
            if (!OnWave || !OnStep)
                return;

            foreach (var info in _waves[_cursor].info)
            {
                Emit(info);
            }

            _currentTime = 0f;
        }

        /// <summary>
        /// 생성 이벤트 데이터를 스트림에 전송 
        /// </summary>
        /// <param name="data">생성 정보 데이터</param>
        private void Emit(SpawnInfoData data)
        {
            int size = 0;
            foreach (var detail in data.details)
            {
                size += detail.count;
            }

            var entities = new UnitEntity[size];
            var evData = new SpawnEventData
            {
                spawnAt = data.spawnAt,
                area = data.area,
                entities = entities
            };

            int streamDataIndex = 0;
            int spawnDataIndex = 0;
            while (streamDataIndex < size)
            {
                int count = data.details[spawnDataIndex].count;
                if (_db.TryFind(data.details[spawnDataIndex].id, out var entity))
                {
                    for (int i = 0; i < count; ++i)
                    {
                        entities[streamDataIndex] = entity;
                    }

                    ++streamDataIndex;
                }
                else
                    streamDataIndex += count;
            }
            
            _stream.Emit(evData);
        }
    }
}