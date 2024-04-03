using System;
using System.Collections.Generic;
using RGLabs.InGame.System;

namespace RGLabs.Common
{
    public class DataStream<T> : IUpdate
    {
        public event Action<T> Collect;
        
        private readonly Queue<T> _queue = new();

        public void Init()
        {
            _queue.Clear();
        }

        public void Emit(T item) => _queue.Enqueue(item);

        public void ProcessUpdate(float _)
        {
            if (_queue.Count == 0)
                return;
            
            Collect?.Invoke(_queue.Dequeue());
        }
    }
}