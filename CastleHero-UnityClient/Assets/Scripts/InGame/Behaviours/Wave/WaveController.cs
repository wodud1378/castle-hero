using System.Collections.Generic;
using RGLabs.Common;
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
        
        private readonly List<IUpdate> _updates = new();
        
        private void Awake()
        {
            var spawnEventStream = new DataStream<SpawnEvent>();
            
            _updates.Add(spawnEventStream);

            var waveUpdate = new WaveUpdate(_dbReference.waves, _dbReference.monsters, spawnEventStream);
            _updates.Add(waveUpdate);

            foreach (var area in _spawnAreas)
            {
                var creationStream = new DataStream<CreationRequest[]>();
                area.Init(spawnEventStream, creationStream);
                
                _updates.Add(creationStream);
            }
        }

        private void Update()
        {
            foreach (var update in _updates)
            {
                update.ProcessUpdate(Time.deltaTime);
            }
        }
    }
}