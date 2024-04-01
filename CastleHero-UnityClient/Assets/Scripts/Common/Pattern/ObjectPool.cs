using System;
using System.Collections.Generic;

namespace RGLabs.Common.Pattern
{
    public interface IObjectPoolItem
    {   
        public void Activate();
        public void Inactivate();
    }

    public class ObjectPool<T> where T : IObjectPoolItem
    {
        public T Key { get; private set; }

        private readonly Func<T> _creationMethod;
        private readonly List<T> _activated;
        private readonly Queue<T> _spares;

        private bool HasSpare => _spares.Count > 0;

        // Lock Default Constructor.
        private ObjectPool()
        {
        }

        public ObjectPool(T key, Func<T> creationMethod)
        {
            Key = key;
            _creationMethod = creationMethod;
            _activated = new List<T>();
            _spares = new Queue<T>();
        }

        public T Get()
        {
            T obj;
            if (HasSpare)
                obj = _spares.Dequeue();
            else
                obj = _creationMethod.Invoke();

            obj.Activate();
            _activated.Add(obj);
            return obj;
        }

        public void Return(T obj)
        {
            obj.Inactivate();
            
            _activated.Remove(obj);
            _spares.Enqueue(obj);
        }
    }
}