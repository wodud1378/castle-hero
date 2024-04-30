using RGLabs.Data.DB;
using RGLabs.Data.Model;
using UniRx;

namespace RGLabs.InGame.System.Wave
{
    public class WaveUpdate : IUpdate
    {
        public bool Updated { get; private set; }
        
        private readonly SpawnEventProvider[] _waves;
        
        public WaveUpdate(WaveEntity[] data)
        {
            int count = data.Length;

            _waves = new SpawnEventProvider[count];
            for (int i = 0; i < count; ++i)
            {
                _waves[i] = new SpawnEventProvider(data[i]);
            }
        }
        
        public void ProcessUpdate(float deltaTime)
        {
            bool isUpdated = false;
            foreach (var wave in _waves)
            {
                if (wave.IsDone)
                    continue;
                
                wave.ProcessUpdate(deltaTime);
                isUpdated = true;
            }

            Updated = isUpdated;
        }
    }
}