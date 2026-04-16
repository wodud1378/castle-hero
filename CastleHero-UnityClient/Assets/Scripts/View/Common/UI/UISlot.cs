using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Data;
using CastleHero.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CastleHero.Common.Sound;
using CastleHero.Common.Pattern;

namespace CastleHero.View.Common.UI
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

            if (icon != null)
                icon.enabled = false;

            Sprite sprite = null;
            if (!string.IsNullOrEmpty(spritePath))
            {
                try
                {
                    sprite = await spritePath.Load<Sprite>(_ctSource.Token);
                }
                catch
                {
                    Debug.LogError($"Sprite Not Found. path=\"{spritePath}\"");
                }
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

            if (icon != null)
                icon.sprite = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnClick == null)
                return;

            ServiceLocator.Get<ISoundManager>().PlaySfx(ServiceLocator.Get<SoundPath>().button);
            OnClick.Invoke(this);
        }
    }
}