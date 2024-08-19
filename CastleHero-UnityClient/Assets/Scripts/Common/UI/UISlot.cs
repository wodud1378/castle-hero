using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UISlot : UIState, IDisposable, IPointerClickHandler
    {
        public event Action<UISlot> OnClick;
        public Image icon;
        public TMP_Text label;

        private CancellationTokenSource _ctSource;

        public async UniTask Init(string spritePath, string text = "")
        {
            _ctSource?.Cancel();
            _ctSource = new();

            icon.enabled = false;

            Sprite sprite;
            try
            {
                sprite = await spritePath.Load<Sprite>(_ctSource.Token);
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

            if (label != null)
                label.text = text;
        }

        public virtual void Dispose()
        {
            _ctSource?.Cancel();
            _ctSource?.Dispose();
            _ctSource = null;

            icon.sprite = null;
        }

        public void OnPointerClick(PointerEventData eventData) => OnClick?.Invoke(this);
    }
}