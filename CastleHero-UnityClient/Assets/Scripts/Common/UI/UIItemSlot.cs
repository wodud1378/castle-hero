using System;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UIItemSlot : MonoBehaviour, IDisposable
    {
        public Image icon;
        public TMP_Text label;

        private AsyncOperationHandle<Sprite> _spriteHandle;

        public async UniTask InitAsync(string spritePath, string text = "")
        {
            icon.enabled = false;

            Sprite sprite;
            try
            {
                _spriteHandle = await spritePath.Handle<Sprite>();
            
                sprite = _spriteHandle.Result;
            }
            catch
            {
                Debug.LogError($"Sprite Not Found. path=\"{spritePath}\"");
                sprite = null;
            }
            
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
            _spriteHandle.Release();
        }
    }
}