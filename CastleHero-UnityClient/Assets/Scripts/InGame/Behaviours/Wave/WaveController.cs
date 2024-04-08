using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.System;
using RGLabs.InGame.System.Wave;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Wave
{
    public class WaveController : MonoBehaviour
    {
        [SerializeField] private DBReference _dbReference;
        [SerializeField] private SpawnArea[] _spawnAreas;

        public bool IsRunning { get; set; } = false;
        
        private readonly List<IUpdate> _updates = new();
        
        private void Awake()
        {
            var spawnStream = new DataStream<SpawnEvent>();
            var releaseStream = new DataStream<UnitBehaviour>();
            var waveUpdate = new WaveUpdate(_dbReference.waves, _dbReference.monsters, spawnStream, releaseStream);

            _updates.Add(spawnStream);
            _updates.Add(releaseStream);
            _updates.Add(waveUpdate);

            foreach (var area in _spawnAreas)
            {
                var creationStream = new DataStream<CreationRequest[]>();
                area.Init(spawnStream, creationStream, releaseStream);
                
                _updates.Add(creationStream);
            }
        }

        private void Update()
        {
            if (!IsRunning)
                return;
            
            foreach (var update in _updates)
            {
                update.ProcessUpdate(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            foreach (var update in _updates)
            {
                if(update is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }
}