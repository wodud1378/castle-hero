using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RGLabs.Utility
{
    public static class AddressableHelper
    {
        public static async UniTask<T> Instantiate<T>(this AssetReference reference, Transform transform,
            CancellationToken ct = default)
        {
            var task = reference.InstantiateAsync(transform)
                .ToUniTask(cancellationToken: ct)
                .SuppressCancellationThrow();

            var obj = await task;
            return obj.Result == null ? default : obj.Result.GetComponent<T>();
        }

        public static async UniTask<T> Instantiate<T>(this string path, Transform transform,
            CancellationToken ct = default)
        {
            var task = Addressables.InstantiateAsync(path, transform)
                .ToUniTask(cancellationToken: ct)
                .SuppressCancellationThrow();

            var obj = await task;
            return obj.Result == null ? default : obj.Result.GetComponent<T>();
        }

        public static async UniTask<T> Load<T>(this string path, CancellationToken cancellationToken = default)
        {
            var handle = Addressables.LoadAssetAsync<T>(path);
            await handle
                .ToUniTask(cancellationToken: cancellationToken)
                .SuppressCancellationThrow();

            return handle.Result;
        }

        public static void Release<T>(this AsyncOperationHandle<T> handle)
        {
            if (!handle.IsValid())
                return;

            Addressables.Release(handle);
        }

        public static void Release(this AsyncOperationHandle handle)
        {
            if (!handle.IsValid())
                return;

            Addressables.Release(handle);
        }
    }
}