using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using RGLabs.Data.Model;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Data.DB
{
    public interface IDataBase
    {
        public int Id { get; set; }
        public void Load(object[] data);
    }

    public class DataFieldAttribute : Attribute
    {
        public string Name { get; }
        public int Index { get; }

        public DataFieldAttribute(string name, int index = -1)
        {
            Name = name;
            Index = index;
        }
    }

    public class DBAttribute : Attribute
    {
        public string ChartName { get; }

        public DBAttribute(string chartName)
        {
            ChartName = chartName;
        }
    }

    public abstract class DB<T> : IDataBase where T : IEntity, new()
    {
        public int Id { get; set; }

        protected T[] entities;

        private readonly Dictionary<int, int> _resultCache = new();

        public int Length => entities.Length;

        public T this[int index]
        {
            get
            {
                TryIndexOf(index, out var entity);
                return entity;
            }
        }

        public IEnumerable<T> Where(Predicate<T> condition) => entities.Where(condition.Invoke);

        public bool TryIndexOf(int index, out T entity)
        {
            if (!index.IsValidIndex(entities))
            {
                entity = FallBackEntity();
                return false;
            }

            entity = entities[index];
            return true;
        }

        public bool TryFind(int id, out T entity)
        {
            if (!_resultCache.TryGetValue(id, out int index))
            {
                index = Array.FindIndex(entities, (it) => id == it.Id);
                _resultCache[id] = index;
            }

            if (index < 0)
            {
                entity = FallBackEntity();
                return false;
            }

            entity = entities[index];
            return true;
        }

        public T Find(Predicate<T> predicate) => Array.Find(entities, predicate);
        
        public List<T> FindAll(Predicate<T> predicate)
        {
            var list = new List<T>();
            ForEach(x =>
            {
                if (!predicate.Invoke(x))
                    return;

                list.Add(x);
            });

            return list;
        }

        public bool TryFind(Predicate<T> predicate, out T entity)
        {
            int index = 0;
            while (index.IsValidIndex(entities))
            {
                if (predicate.Invoke(entities[index]))
                {
                    entity = entities[index];
                    return true;
                }

                ++index;
            }

            entity = FallBackEntity();
            return false;
        }

        public bool TryFindIndex(int id, out int index)
        {
            index = Array.FindIndex(entities, (x) => x.Id == id);

            return IsValidIndex(index);
        }

        public bool IsValidIndex(int index) => index.IsValidIndex(entities);

        public void ForEach(Action<T> action)
        {
            foreach (var entity in entities)
            {
                action.Invoke(entity);
            }
        }

        public void BinarySearch(Action<T> onFound)
            => BinarySearch(onFound, 0, Length);
        
        public T BinarySearch(Predicate<T> predicate)
            => BinarySearch(predicate, 0, Length);

        private void BinarySearch(Action<T> action, int startIndex, int count)
        {
            if (count == 0)
                return;

            int midIndex = startIndex + count / 2;
            T midData = entities[midIndex];

            // 중간 데이터에 대해 Action 실행
            action(midData);

            // 왼쪽 절반 탐색
            BinarySearch(action, startIndex, count / 2);

            // 오른쪽 절반 탐색
            BinarySearch(action, midIndex + 1, count - (count / 2) - 1);
        }
        
        private T BinarySearch(Predicate<T> predicate, int startIndex, int count)
        {
            if (count == 0)
                return FallBackEntity();

            int midIndex = startIndex + count / 2;
            T midData = entities[midIndex];

            if (predicate(midData))
            {
                return midData;
            }

            var leftResult = BinarySearch(predicate, startIndex, count / 2);
            if (leftResult != null)
            {
                return leftResult;
            }

            return BinarySearch(predicate, midIndex + 1, (count - 1) / 2);
        }

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

        public IEnumerable<T> Map(IEnumerable<int> ids)
        {
            var result = new List<T>();
            foreach (var id in ids)
            {
                if (TryFind(id, out var entity))
                    result.Add(entity);
            }

            return result.ToArray();
        }

        public void Load(object[] data)
        {
            int length = data.Length;
            entities = new T[length];

            for (int i = 0; i < length; ++i)
            {
                Convert(data[i], ref entities[i]);
            }
        }

        public T FallBackEntity()
        {
            return new() { IsValid = false };
        }

        protected virtual void Convert(object from, ref T to)
        {
            to = (T)from;
            to.IsValid = true;
        }
    }
}