using RGLabs.InGame.Data.DB;
using RGLabs.InGame.System.Wave;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Wave
{
    public class WaveController : MonoBehaviour
    {
        [SerializeField] private DBReference _dbReference;
        [SerializeField] private SpawnArea[] _spawnAreas;

        public int SpawnAreaCount => _spawnAreas.Length;

        public bool isRunning;
        
        private WaveUpdate _update;
        
        private void Awake()
        {
            var spawnStream = InGameContext.streams.spawnEvent;
            var releaseStream = InGameContext.streams.release;
            _update = new WaveUpdate(_dbReference.waves, _dbReference.monsters, spawnStream, releaseStream);

            for (int i = 0, count = SpawnAreaCount; i < count; ++i)
            {
                var creationStream = InGameContext.streams.creations[i];
                _spawnAreas[i].Init(spawnStream, creationStream, releaseStream);
            }

            isRunning = false;
        }

        private void Update()
        {
            if (!isRunning)
                return;
            
            _update.ProcessUpdate(Time.deltaTime);
        }
    }
}