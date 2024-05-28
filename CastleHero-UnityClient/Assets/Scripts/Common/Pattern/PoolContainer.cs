using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using UnityEngine;

namespace RGLabs.Common.Pattern
{
    public class PoolContainer : IDisposable
    {
        private readonly Dictionary<string, AddressablePool<PoolItemBase>> _pools = new();
        
        public AddressablePool<PoolItemBase> Get(string resourcePath, bool autoCreate = true)
        {
            if (!_pools.TryGetValue(resourcePath, out var pool))
            {
                if (!autoCreate)
                    return null;
                
                pool = new AddressablePool<PoolItemBase>(resourcePath);
                _pools[resourcePath] = pool;
            }
            
            return pool;
        }

        public async UniTask<T> GetItem<T>(string resourcePath, bool autoCreatePool = true) where T : PoolItemBase
        {
            return await GetItem<T>(resourcePath, default, autoCreatePool);
        }
        
        public async UniTask<T> GetItem<T>(string resourcePath, Vector2 position, bool autoCreatePool = true) where T : PoolItemBase
        {
            var item = await GetItem(resourcePath, position, autoCreatePool);
            if (item == null)
                return null;
            
            return item as T;
        }
        
        public async UniTask<PoolItemBase> GetItem(string resourcePath, bool autoCreatePool = true)
        {
            return await GetItem(resourcePath, default, autoCreatePool);
        }
        
        public async UniTask<PoolItemBase> GetItem(string resourcePath, Vector2 position, bool autoCreatePool = true)
        {
            var pool = Get(resourcePath, autoCreatePool);
            var item = await pool.Get(position);
            if (item == null)
                return null;
            
            item.Container = this;
            item.Pool = pool;
            
            return item;
        }

        public void Release(PoolItemBase poolItemBase)
        {
            var pool = Get(poolItemBase.ResourcePath,false);

            pool?.Release(poolItemBase);
        }

        public void Release(string key)
        {
            var pool = Get(key,false);

            pool?.Dispose();
        }

        public void Dispose()
        {
            foreach (var pair in _pools)
            {
                pair.Value.Dispose();
            }
            
            _pools.Clear();
        }
    }
}