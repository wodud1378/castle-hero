using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours.Unit;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Common.Pattern
{
    public interface IObjectPoolItem
    {
        public string ResourcePath { get; set; }
        
        public void Activate();
        public void Inactivate();
    }
    
    public class AddressablePool<T> : IDisposable where T : MonoBehaviour, IObjectPoolItem
    {
        private readonly List<T> _activated;
        private readonly Queue<T> _spares;

        private readonly string _path;

        private bool HasSpare => _spares.Count > 0;

        public AddressablePool(string path)
        {
            _path = path;
            _activated = new List<T>();
            _spares = new Queue<T>();
        }

        public async UniTask<T> Get(Vector2 position = default)
        {
            T obj;
            if (HasSpare)
                obj = _spares.Dequeue();
            else
            {
                var go = await Addressables.InstantiateAsync(_path);
                obj = go.GetComponent<T>();
                obj.ResourcePath = _path;
            }

            obj.transform.position = position;
            obj.Activate();
            _activated.Add(obj);

            return obj;
        }

        public void Release(T obj)
        {
            obj.Inactivate();
            _spares.Enqueue(obj);
        }

        public void Dispose()
        {
            foreach (var obj in _activated)
                Release(obj);

            foreach (var spare in _spares)
            {
                Addressables.ReleaseInstance(spare.gameObject);
            }
        }
    }
}