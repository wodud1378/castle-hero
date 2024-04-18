using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.InGame.System;
using RGLabs.InGame.System.Wave;

namespace RGLabs.InGame
{
    public class Streams : IDisposable
    {
        public readonly DataStream<SpawnEvent[]> spawnEvent;
        public readonly DataStream<ReleaseEvent> release;
        public readonly DataStream<AdjustHpEvent> atk;
        public readonly DataStream<AdjustHpEvent> heal;

        private readonly IList<IUpdate> _updates;
        private readonly IList<IDisposable> _disposables;
        
        public Streams()
        {
            _updates = new List<IUpdate>();
            _disposables = new List<IDisposable>();
            
            spawnEvent = DataStream<SpawnEvent[]>.Create(_updates, _disposables);
            release = DataStream<ReleaseEvent>.Create(_updates, _disposables);
            atk = DataStream<AdjustHpEvent>.Create(_updates, _disposables);
            heal = DataStream<AdjustHpEvent>.Create(_updates, _disposables);
        }
        
        public void Update()
        {
            foreach (var update in _updates)
            {
                update.ProcessUpdate(0f);
            }
        }
        
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            
            _disposables.Clear();
            _updates.Clear();
        }
    }
}