using System;
using System.Collections.Generic;

namespace RGLabs.InGame.System.Wave
{
    public class DataStream<T> : ISystem
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