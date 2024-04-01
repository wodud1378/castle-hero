using System;
using RGLabs.InGame.System.Wave;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public class WaveHandler : MonoBehaviour
    {
        [SerializeField] private float _startY;
        [SerializeField] private Vector2 _xRange;
        
        [SerializeField] private WaveLoop _waveLoop;
        [SerializeField] private WaveSpawnLoop _spawnLoop;

        private void Awake()
        {
            _waveLoop.Init();
            _spawnLoop.Init();
            
            _spawnLoop.MonsterSpawned += OnMonsterSpawned;
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            _waveLoop.ProcessUpdate(delta);
            _spawnLoop.ProcessUpdate(delta);
        }

        private void OnMonsterSpawned(GameUnit unit)
        {
        }
    }
}