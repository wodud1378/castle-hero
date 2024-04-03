using System;
using System.Collections.Generic;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.Data.DB
{
    [Serializable]
    public abstract class DB<T> where T : IEntity
    {
        [SerializeField] protected T[] _entities;

        private readonly Dictionary<int, int> _resultCache = new();
        
        public bool TryFind(int id, out T entity)
        {
            if (!_resultCache.TryGetValue(id, out int index))
            {
                index = Array.FindIndex(_entities, (it) => id == it.Id);
                _resultCache[id] = index;
            }

            if (index < 0)
            {
                entity = FallBackEntity();
                return false;
            }

            entity = _entities[index];
            return true;
        }

        protected abstract T FallBackEntity();

        public void ClearCache()
        {
            _resultCache.Clear();
        }
    }
}