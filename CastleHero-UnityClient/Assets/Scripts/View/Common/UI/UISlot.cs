using System;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Data;
using CastleHero.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        private ISoundManager _sounds;
        private SoundPath _soundPath;

        protected override void OnAwake()
        {
            base.OnAwake();
            var sl = ServiceLocator.Instance;
            _sounds = sl.Get<ISoundManager>();
            _soundPath = sl.Get<SoundPath>();
        }

        public void Init(string spritePath, string text = "")
        {
            Sprite sprite = null;
            if (!string.IsNullOrEmpty(spritePath))
            {
                var handle = Addressables.LoadAssetAsync<Sprite>(spritePath);
                sprite = handle.WaitForCompletion();
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
            if (icon != null)
                icon.sprite = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnClick == null)
                return;

            _sounds.PlaySfx(_soundPath.button);
            OnClick.Invoke(this);
        }
    }
}