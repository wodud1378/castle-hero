using System;
using System.Collections.Generic;
using RGLabs.Common.Behaviours;

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