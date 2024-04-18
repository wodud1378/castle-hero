using System;
using System.Collections.Generic;
using RGLabs.InGame.System;
using UnityEngine;

namespace RGLabs.Common
{
    public class DataStream<T> : IUpdate, IDisposable
    {
        public event Action<T> Collect;

        private readonly Queue<T> _queue = new();

        public int processPerFrame;

        private DataStream(int processPerFrame) => this.processPerFrame = processPerFrame;
        
        public void Emit(T item) => _queue.Enqueue(item);

        public void ProcessUpdate(float _)
        {
            if (_queue.Count == 0)
                return;

            int count = Mathf.Min(processPerFrame, _queue.Count);
            for (int i = 0; i < count; ++i)
            {
                Collect?.Invoke(_queue.Dequeue());
            }
        }

        public void Dispose()
        {
            _queue.Clear();
            Collect = null;
        }

        public static DataStream<T> Create(int processPerFrame = 1) => new(processPerFrame);

        public static DataStream<T> Create(IList<IUpdate> updateDependency, IList<IDisposable> disposeDependency)
        {
            var stream = Create();
            updateDependency.Add(stream);
            disposeDependency.Add(stream);
            
            return stream;
        }
    }
}