using System;
using System.Collections.Generic;
using CastleHero.Common.Behaviours;
using UnityEngine;

namespace CastleHero.Common.Pattern
{
    public interface IObjectPoolItem
    {
        PoolContainer Container { get; set; }
        string ResourcePath { get; set; }
        bool Activated { get; }
        void Activate();
        void Inactivate();
    }

    /// <summary>
    /// 이미 로드된 프리팹을 받아 동기 Instantiate 로 풀 관리.
    /// 어드레서블 리소스 핸들 관리는 PoolContainer 가 책임.
    /// </summary>
    public class AddressablePool<T> : IDisposable where T : MonoBehaviour, IObjectPoolItem
    {
        private readonly GameObject _prefab;
        private readonly string _path;
        private readonly Transform _parent;
        private readonly List<T> _activated = new();
        private readonly List<T> _spares = new();

        public AddressablePool(string path, GameObject prefab, Transform parent = null)
        {
            _path = path;
            _prefab = prefab;
            _parent = parent;
        }

        public void Preload(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = Spawn();
                if (obj == null) return;
                obj.Inactivate();
                _spares.Add(obj);
            }
        }

        public void ForceActivate(T obj)
        {
            if (obj.Activated) return;
            _spares.Remove(obj);
            obj.Activate();
            if (!_activated.Contains(obj)) _activated.Add(obj);
        }

        public bool TryGet(out T item, Vector2 position = default)
        {
            T obj;
            if (_spares.Count > 0)
            {
                int last = _spares.Count - 1;
                obj = _spares[last];
                _spares.RemoveAt(last);
            }
            else
            {
                obj = Spawn();
                if (obj == null)
                {
                    item = null;
                    return false;
                }
            }

            obj.transform.position = position;
            obj.Activate();
            _activated.Add(obj);
            item = obj;
            return true;
        }

        public void Release(T obj)
        {
            obj.Inactivate();
            _activated.Remove(obj);
            _spares.Add(obj);
        }

        public void Dispose()
        {
            foreach (var obj in _activated)
                if (obj != null) UnityEngine.Object.Destroy(obj.gameObject);
            foreach (var obj in _spares)
                if (obj != null) UnityEngine.Object.Destroy(obj.gameObject);

            _activated.Clear();
            _spares.Clear();
        }

        private T Spawn()
        {
            if (_prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[AddressablePool] prefab is null for path '{_path}'");
#endif
                return null;
            }

            var go = UnityEngine.Object.Instantiate(_prefab, _parent);
            var comp = go.GetComponent<T>();
            if (comp == null)
            {
                UnityEngine.Object.Destroy(go);
                return null;
            }

            comp.ResourcePath = _path;
            return comp;
        }
    }
}
