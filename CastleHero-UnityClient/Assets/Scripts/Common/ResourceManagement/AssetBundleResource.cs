using System;
using System.Collections.Generic;
using RGLabs.InGame.Behaviours;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace RGLabs.Common.ResourceManagement
{
    public class AssetBundleResource : IResource
    {
        private readonly Dictionary<string, AsyncOperationHandle> _resourceCache = new();

        public void PreLoad(string path)
        {
            if (!_resourceCache.TryGetValue(path, out var h))
            {
                var aoHandle = Addressables.LoadAssetAsync<Object>(path);
                aoHandle.CompletedTypeless += (typeless) => _resourceCache[path] = typeless;
            }
        }

        public void Release(string path)
        {
            
        }

        public void Instantiate<T>(string path, Action<T> onComplete) where T : Obj
        {
            Addressables.InstantiateAsync(path).Completed += handle =>
            {
                var res = handle.Result;
                var component = res.GetComponent<T>();
                onComplete?.Invoke(component);
            };
        }

        public void Destroy(Obj obj)
        {
            Addressables.ReleaseInstance(obj.gameObject);
        }
        
        public void Load<T>(string path, Action<T> onLoadComplete) where T : Object
        {
            if (!_resourceCache.TryGetValue(path, out var h))
            {
                var aoHandle = Addressables.LoadAssetAsync<T>(path);
                aoHandle.CompletedTypeless += (typeless) => _resourceCache[path] = typeless;
                aoHandle.Completed += handle => onLoadComplete.Invoke(handle.Result);
            }
            else
                onLoadComplete.Invoke(h.Convert<T>().Result);
        }
    }
}