using RGLabs.Data.Model;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.InGame.System.Wave
{
    public class SpawnEventProvider : IUpdate
    {
        private class Progress
        {
            public readonly SpawnInfo info;
            public readonly int spawnPerOnce;
            
            public float currentTime;
            public int spawned;

            public int Left => info.count - spawned;
            public bool IsEnd => Left <= 0;
            
            public Progress(SpawnInfo info, Pattern pattern)
            {
                this.info = info;
                spawnPerOnce = pattern == Pattern.AtOnce ? info.count : 1;
                currentTime = info.timeStep;
                spawned = 0;
            }
        }
        
        public bool IsDone { get; private set; }
        
        private readonly Progress[] _progresses;
        private readonly SpawnEvent[][] _buffers;
        private readonly int _infoLength;
        private readonly float _startTime;

        private float _timeSinceActive;

        public SpawnEventProvider(WaveEntity data)
        {
            _startTime = data.startTime;
            _infoLength = data.info.Length;
            _progresses = new Progress[_infoLength];
            _buffers = new SpawnEvent[_infoLength][];

            for (int i = 0; i < _infoLength; ++i)
            {
                _progresses[i] = new Progress(data.info[i], data.pattern);
            }

            for (int i = 0; i < _infoLength; ++i)
            {
                _buffers[i] = new SpawnEvent[_progresses[i].spawnPerOnce];
            }
            
            _timeSinceActive = 0f;

            IsDone = false;
        }

        public void ProcessUpdate(float deltaTime)
        {
            _timeSinceActive += deltaTime;
            if (_timeSinceActive < _startTime)
                return;

            bool updated = false;
            for (int i = 0; i < _infoLength; ++i)
            {
                var progress = _progresses[i];
                if (progress.IsEnd)
                    continue;
                
                updated = true;
                
                progress.currentTime += deltaTime;
                if (progress.currentTime < progress.info.timeStep)
                    continue;

                int bufferIndex = 0;
                int left = Mathf.Min(progress.spawnPerOnce, progress.Left);
                while (left > 0)
                {
                    _buffers[i][bufferIndex++].Set(progress.info.id, progress.info.lv, progress.info.area);
                    --left;
                    
                    ++progress.spawned;
                }
                
                progress.currentTime = 0f;
                _buffers[i].Publish();
            }

            if (!updated)
                IsDone = true;
        }
    }
}