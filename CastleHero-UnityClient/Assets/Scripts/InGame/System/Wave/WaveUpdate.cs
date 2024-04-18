using RGLabs.InGame.Behaviours;

namespace RGLabs.InGame.System.Wave
{
    public class WaveUpdate : IUpdate
    {
        private readonly SpawnEventProvider[] _waves;
        
        public WaveUpdate()
        {
            var data = InGameContext.db.waves;
            
            int count = data.Length;
            _waves = new SpawnEventProvider[count];
            for (int i = 0; i < count; ++i)
            {
                _waves[i] = new SpawnEventProvider(data[i]);
            }

            InGameContext.streams.release.Collect += OnCollectData;
        }

        private void OnCollectData(ReleaseEvent data) => data.unit.DestroySelf();

        public void ProcessUpdate(float deltaTime)
        {
            foreach (var wave in _waves)
            {
                if (wave.IsDone)
                    continue;
                
                wave.ProcessUpdate(deltaTime);
            }
        }
    }
}