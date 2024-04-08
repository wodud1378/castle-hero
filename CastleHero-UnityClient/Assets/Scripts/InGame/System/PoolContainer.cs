using System;
using System.Collections.Generic;
using RGLabs.Common.Pattern;
using RGLabs.InGame.Behaviours;

namespace RGLabs.InGame.System
{
    public class PoolContainer : IDisposable
    {
        private Dictionary<string, AddressablePool<Obj>> _pools = new();
        
        public AddressablePool<Obj> Get(string resourcePath, bool autoCreate = true)
        {
            if (!_pools.TryGetValue(resourcePath, out var pool))
            {
                if (!autoCreate)
                    return null;
                
                pool = new AddressablePool<Obj>(resourcePath);
                _pools[resourcePath] = pool;
            }
            
            return pool;
        }

        public void Release(Obj obj)
        {
            var pool = Get(obj.ResourcePath,false);

            pool?.Release(obj);
        }

        public void Release(string key)
        {
            var pool = Get(key,false);

            pool?.Dispose();
        }

        public void Dispose()
        {
            _pools.Clear();
            _pools = null;
        }
    }
}