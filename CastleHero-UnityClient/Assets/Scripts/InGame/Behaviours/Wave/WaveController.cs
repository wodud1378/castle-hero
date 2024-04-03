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
            var creationRequestStream = new DataStream<CreationRequest[]>();
            
            _updates.Add(spawnEventStream);
            _updates.Add(creationRequestStream);

            var waveUpdate = new WaveUpdate(_dbReference.waves, _dbReference.monsters, spawnEventStream);
            _updates.Add(waveUpdate);

            foreach (var area in _spawnAreas)
            {
                area.Init(spawnEventStream);
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