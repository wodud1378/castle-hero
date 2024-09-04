using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    [RequireComponent(typeof(Sprite))]
    public class AddressableImage : MonoBehaviour, IDisposable
    {
        public Image image;

        private CancellationTokenSource _ctSource;
        private AsyncOperationHandle<Sprite> _handle;

        public async UniTask Set(string path)
        {
            _ctSource?.Cancel();
            _ctSource = new();
            
            if (string.IsNullOrEmpty(path))
            {
                image.enabled = false;
                return;
            }

            _handle = Addressables.LoadAssetAsync<Sprite>(path);
            var sprite = await _handle.ToUniTask(cancellationToken:_ctSource.Token);

            image.sprite = sprite;
            image.enabled = sprite != null;
        }
        
        public void Dispose()
        {
            image.sprite = null;
            image.enabled = false;
            
            _ctSource?.Cancel();
            _handle.Release();
        }

        private void OnValidate()
        {
            if(image == null)
                image = GetComponent<Image>();
        }
    }
}