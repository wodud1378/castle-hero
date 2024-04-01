using System;
using System.Collections.Generic;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.System.MonsterFactory;
using RGLabs.InGame.System.Spawn;
using UnityEngine;

namespace RGLabs.InGame.System.Wave
{
    [Serializable]
    public class WaveSpawnLoop : IUpdateLoop
    {
        public event Action<GameUnit> MonsterSpawned;

        [SerializeField] private SpawnStream _stream;
        [SerializeField] private int _limitPerFrame;
        
        private IMonsterFactory _factory;
        
        public void Init()
        {
            _factory = new DefaultMonsterFactory();
        }
        
        public void ProcessUpdate(float deltaTime)
        {
            int repeat = Mathf.Min(_limitPerFrame, _stream.Count);
            while (repeat > 0)
            {
                var entity = _stream.Dequeue();
                _factory.PushCreationRequest(entity, (unit)=> MonsterSpawned?.Invoke(unit));
                --repeat;
            }
        }
    }
}