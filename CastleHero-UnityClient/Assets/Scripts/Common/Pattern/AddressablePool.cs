using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Common.Pattern
{
    public interface IObjectPoolItem
    {
        public PoolContainer Container { get; set; }
        
        public string ResourcePath { get; set; }
        
        public bool Activated { get; }
        
        public void Activate();
        public void Inactivate();
    }
    
    public class AddressablePool<T> : IDisposable where T : MonoBehaviour, IObjectPoolItem
    {
        private readonly List<T> _activated;
        private readonly List<T> _spares;

        private readonly string _path;
        private readonly Transform _parent;

        private bool HasSpare => _spares.Count > 0;

        public AddressablePool(string path, Transform parent = null)
        {
            _path = path;
            _activated = new List<T>();
            _spares = new List<T>();
            _parent = parent;
        }

        public void ForceActivate(T obj)
        {
            if (obj.Activated)
                return;

            if (_spares.Contains(obj))
                _spares.Remove(obj);
            
            obj.Activate();
            _activated.Add(obj);
        }

        public async UniTask<T> Get(Vector2 position = default)
        {
            T obj;
            if (HasSpare)
            {
                obj = _spares[0];
                _spares.RemoveAt(0);
            }
            else
            {
                var go = await _path.Instantiate<T>(_parent);
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
            _spares.Add(obj);
        }

        public void ClearSpares()
        {
            foreach (var spare in _spares)
            {
                Addressables.ReleaseInstance(spare.gameObject);
            }
        }
        
        public void Dispose()
        {
            foreach (var obj in _activated)
                Release(obj);

            ClearSpares();
            
            _activated.Clear();
            _spares.Clear();
        }
    }
}