using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UIItemSlot : MonoBehaviour, IDisposable, IPointerClickHandler
    {
        [SerializeField] private Graphic _raycastTarget;
        
        public event Action<UIItemSlot> OnClick;

        public Image icon;
        public TMP_Text label;

        public bool ReceiveRay
        {
            set { if (_raycastTarget != null) _raycastTarget.enabled = value; }
        }

        public async UniTask InitAsync(string spritePath, string text, CancellationToken ct)
        {
            icon.enabled = false;

            Sprite sprite;
            try
            {
                sprite = await spritePath.Load<Sprite>(ct);
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
            icon.sprite = null;
        }

        public void OnPointerClick(PointerEventData eventData) => OnClick?.Invoke(this);
    }
}