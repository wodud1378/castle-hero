using System;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.System.Wave
{
    [Serializable]
    public struct WaveDetail
    {
        public int monsterId;
        public int count;
    }
    
    [Serializable]
    public struct Wave
    {
        public float time;
        public WaveDetail[] details;
        
        [HideInInspector]
        public bool isDone;
    }
    
    [Serializable]
    public class WaveLoop : IUpdateLoop
    {
        [SerializeField] private SpawnStream _stream;
        
        [Header("Monster Set Up")]
        [SerializeField] private MonsterDB _db;

        [Header("Waves")] 
        [SerializeField] private Wave[] _waves;
        
        private int _cursor = 0;
        private float _currentTime = 0f;

        public void Init()
        {
            _db.ClearCache();
        }

        public void ProcessUpdate(float deltaTime)
        {
            _currentTime += deltaTime;
            
            UpdateCursor();
            ProcessWave();
        }

        private void UpdateCursor()
        {
            if (!_waves[_cursor].isDone)
                return;

            if (_cursor + 1 >= _waves.Length)
                return;

            if (_currentTime < _waves[_cursor + 1].time)
                return;

            ++_cursor;
        }

        private void ProcessWave()
        {
            if (_currentTime < _waves[_cursor].time)
                return;
            
            if (_waves[_cursor].isDone)
                return;

            foreach (var detail in _waves[_cursor].details)
            {
                if (!_db.TryFind(detail.monsterId, out var entity))
                    continue;
                
                _stream.Enqueue(entity);
            }
            
            _waves[_cursor].isDone = true;
        }
    }
}