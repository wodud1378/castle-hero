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
                currentTime = 0f;
                spawned = 0;
            }
        }

        public bool IsDone { get; private set; }

        private readonly Progress[] _progresses;
        private readonly SpawnEvent[][] _buffers;
        private readonly int _length;
        private readonly float _startTime;

        private float _timeSinceActive;

        public SpawnEventProvider(WaveEntity data)
        {
            _startTime = data.startTime;
            _length = Mathf.Min(
                data.ids.Length,
                data.counts.Length,
                data.lvs.Length,
                data.areas.Length,
                data.timeSteps.Length);

            _progresses = new Progress[_length];
            _buffers = new SpawnEvent[_length][];

            for (int i = 0; i < _length; ++i)
            {
                var info = new SpawnInfo
                {
                    id = data.ids[i],
                    count = data.counts[i],
                    lv = data.lvs[i],
                    area = data.areas[i],
                    timeStep = data.timeSteps[i]
                };

                _progresses[i] = new Progress(info, (Pattern)data.pattern);
            }

            for (int i = 0; i < _length; ++i)
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
            for (int i = 0; i < _length; ++i)
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