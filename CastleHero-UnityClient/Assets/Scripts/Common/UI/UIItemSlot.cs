using System;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UIItemSlot : MonoBehaviour, IDisposable
    {
        public Image icon;
        public TMP_Text label;
        public Button button;

        private AsyncOperationHandle<Sprite> _handle;

        public async UniTask InitAsync(string spritePath, string text = "")
        {
            _handle = await spritePath.Handle<Sprite>();
            
            var sprite = _handle.Result;
            
            Init(sprite, text);
        }
        
        private void Init(Sprite sprite = null, string text = "")
        {
            if (icon != null)
            {
                icon.sprite = sprite;
                icon.enabled = sprite != null;    
            }
            
            if(label != null)
                label.text = text;
        }


        public virtual void Dispose()
        {
            _handle.Release();
        }
    }
}