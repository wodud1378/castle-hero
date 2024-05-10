using System;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    [RequireComponent(typeof(Image))]
    public class UIAtlasedSprite : MonoBehaviour, IDisposable
    {    
        public AssetReferenceAtlasedSprite reference;
        public Image image;

        private AsyncOperationHandle<Sprite> _handle;

        public async UniTask Load()
        {
            _handle = reference.LoadAssetAsync();

            var sprite = await _handle.ToUniTask();
            if (sprite == null)
            {
                Fallback();
                return;
            }

            image.sprite = sprite;
            image.enabled = true;
        }

        private void Fallback()
        {
            image.enabled = false;
        }

        public void Dispose()
        {
            _handle.Release();
        }

        private void OnValidate()
        {
            if (image == null)
                image = GetComponent<Image>();
        }
    }
}