using RGLabs.InGame.Data.DB;
using UniRx;

namespace RGLabs.InGame.System.Wave
{
    public class WaveUpdate : IUpdate
    {
        private readonly SpawnEventProvider[] _waves;
        
        public WaveUpdate(WaveDB db)
        {
            int count = db.Length;
            _waves = new SpawnEventProvider[count];
            for (int i = 0; i < count; ++i)
            {
                _waves[i] = new SpawnEventProvider(db[i]);
            }

            MessageBroker.Default.Receive<ReleaseEvent>().Subscribe(OnReceiveReleaseEvent);
        }

        private void OnReceiveReleaseEvent(ReleaseEvent data) => data.unit.DestroySelf();

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