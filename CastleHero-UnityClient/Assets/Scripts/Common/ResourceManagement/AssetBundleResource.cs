using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace RGLabs.Common.ResourceManagement
{
    public interface IAddressableResource<T> : IDisposable
    {
        public UniTask<T> Load(string path);
        public void Release(string path);
    }

    public class Prefab<T> : IAddressableResource<T> where T : MonoBehaviour
    {
        public async UniTask<T> Load(string path)
        {
            var go = await Addressables.InstantiateAsync(path);
            return go.GetComponent<T>();
        }

        public void Release(string path)
        {
        }
        
        public void Dispose() { }
    }

    public class Resource<T> : IAddressableResource<T> where T : Object
    {
        private readonly Dictionary<string, AsyncOperationHandle<T>> _cache = new();

        public async UniTask<T> Load(string path)
        {
            if (!_cache.TryGetValue(path, out var handle))
            {
                handle = Addressables.LoadAssetAsync<T>(path);
                await handle.ToUniTask();

                _cache[path] = handle;
            }

            return handle.Result;
        }

        public void Release(string path)
        {
            if (!_cache.Remove(path, out var handle))
                return;

            Addressables.Release(handle);
        }

        public void Dispose()
        {
            var keys = _cache.Keys;
            foreach (var key in keys)
            {
                Release(key);
            }
            
            _cache.Clear();
        }
    }
}