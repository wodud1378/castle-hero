using System;
using System.Collections.Generic;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Utility;
using UnityEngine;

namespace RGLabs.InGame.Data.DB
{
    public abstract class DB<T> : ScriptableObject where T : IEntity
    {
        [field:SerializeField] public int Id { get; set; }
        
        [SerializeField] protected T[] _entities;

        private readonly Dictionary<int, int> _resultCache = new();

        public int Length => _entities.Length;

        public T this[int index]
        {
            get
            {
                TryIndexOf(index, out var entity);
                return entity;
            }
        }
        
        public bool TryIndexOf(int index, out T entity)
        {
            if (!index.IsValidIndex(_entities))
            {
                entity = FallBackEntity();
                return false;
            }

            entity = _entities[index];
            return true;
        }
        
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


        public bool TryFindIndex(int id, out int index)
        {
            index = Array.FindIndex(_entities, (x) => x.Id == id);

            return IsValidIndex(index);
        }
        
        public bool IsValidIndex(int index) => index.IsValidIndex(_entities);
        
        protected abstract T FallBackEntity();

        public void ClearCache()
        {
            _resultCache.Clear();
        }

        public T[] Map(int startIndex, int length)
        {
            length = Mathf.Min(length, Length);
            var array = new T[length];
            for (int i = startIndex; i < length; ++i)
            {
                TryIndexOf(i, out array[i]);
            }

            return array;
        }
        
        public T[] Map(int[] ids)
        {
            int length = ids.Length;
            var array = new T[length];
            for (int i = 0; i < length; ++i)
            {
                TryFind(ids[i], out array[i]);
            }

            return array;
        }
    }
}