using System;
using RGLabs.Data.DB;
using RGLabs.Data.Model;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    [Serializable]
    public class UIUser : UIIconTextSet
    {
    }

    [Serializable]
    public class UIWealth : UIIconTextSet
    {
    }

    [Serializable]
    public class UIIconTextSet : IDisposable
    {
        public Image icon;
        public TMP_Text label;

        private AsyncOperationHandle<Sprite> _spriteHandle;

        public void Set(AsyncOperationHandle<Sprite> handle, string text)
        {
            _spriteHandle = handle;

            Set(handle.Result, text);
        }

        public void Dispose() => _spriteHandle.Release();

        private void Set(Sprite sprite, string text)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            //label.text = text;
        }

        public void Fallback() => Set(null, string.Empty);

        public void SetActive(bool isActive)
        {
            icon.gameObject.SetActive(isActive);
            //label.gameObject.SetActive(isActive);
        }
    }
}