using System.Collections.Generic;
using CastleHero.Common.Behaviours;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CastleHero.Common.Pattern
{
    /// <summary>
    /// IPoolContainer 구현. 어드레서블 핸들은 RegisterWithHandle 로 전달 받아 Dispose 시 함께 해제.
    /// 스테이지 진입 시 Preloader 가 프리팹을 로드하고 Register 로 주입, 종료 시 Dispose 로 일괄 정리.
    /// </summary>
    public class PoolContainer : IPoolContainer
    {
        private readonly Dictionary<string, AddressablePool<PoolItemBase>> _pools = new();
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _handles = new();

        public void Register(string path, GameObject prefab, int preloadCount = 0)
        {
            if (_pools.ContainsKey(path)) return;
            var pool = new AddressablePool<PoolItemBase>(path, prefab);
            if (preloadCount > 0) pool.Preload(preloadCount);
            _pools[path] = pool;
        }

        /// <summary>프리팹 뿐 아니라 Addressables 핸들까지 함께 등록. Dispose 시 핸들 해제.</summary>
        public void RegisterWithHandle(string path, GameObject prefab, AsyncOperationHandle<GameObject> handle, int preloadCount = 0)
        {
            Register(path, prefab, preloadCount);
            _handles[path] = handle;
        }

        public bool TryGet<T>(string resourcePath, out T item, Vector2 position = default) where T : PoolItemBase
        {
            if (!TryGet(resourcePath, out var baseItem, position))
            {
                item = null;
                return false;
            }

            item = baseItem as T;
            return item != null;
        }

        public bool TryGet(string resourcePath, out PoolItemBase item, Vector2 position = default)
        {
            if (!_pools.TryGetValue(resourcePath, out var pool))
            {
                item = null;
                return false;
            }

            if (!pool.TryGet(out item, position))
                return false;

            item.Container = this;
            item.Pool = pool;
            return true;
        }

        public void Release(PoolItemBase obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.ResourcePath)) return;
            if (_pools.TryGetValue(obj.ResourcePath, out var pool))
                pool.Release(obj);
        }

        public bool LoadAndRegister(string path, int preloadCount = 0)
        {
            if (_pools.ContainsKey(path))
                return true;

            var handle = Addressables.LoadAssetAsync<GameObject>(path);
            var prefab = handle.WaitForCompletion();
            if (prefab == null)
                return false;

            RegisterWithHandle(path, prefab, handle, preloadCount);
            return true;
        }

        public void Remove(string path)
        {
            if (!_pools.Remove(path, out var pool))
                return;

            pool.Dispose();

            if (_handles.Remove(path, out var handle) && handle.IsValid())
                Addressables.Release(handle);
        }

        public void Dispose()
        {
            foreach (var pool in _pools.Values) pool.Dispose();
            _pools.Clear();

            foreach (var handle in _handles.Values)
                if (handle.IsValid()) Addressables.Release(handle);
            _handles.Clear();
        }
    }
}
