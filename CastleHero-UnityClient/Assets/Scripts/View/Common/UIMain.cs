using System;
using CastleHero.Common.Sound;
using CastleHero.View.Sound;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CastleHero.Data.Model;

using CastleHero.Common.Pattern;
namespace CastleHero.View.Common
{
    public abstract class UIMain : MonoBehaviour, IDisposable
    {
        public event Action OnOpenAnimationEnd;
        public event Action OnCloseAnimationEnd;

        public bool IsOpen { get; private set; }

        [FormerlySerializedAs("_animator")]
        [SerializeField] private Animator animator;
        [FormerlySerializedAs("_back")]
        [SerializeField] private Button back;

        private readonly int _openHashId = Animator.StringToHash("Entrance");
        private readonly int _closeHashId = Animator.StringToHash("Exit");

        protected virtual void OnAwake()
        {
            if(back != null)
                this.SubscribeButton(back, OnBack, ServiceLocator.Get<SoundPath>().back);
        }

        private void Awake() => OnAwake();

        public void Open()
        {
            IsOpen = true;

            gameObject.SetActive(true);

            OnOpen();
        }

        public void Close()
        {
            IsOpen = false;

            OnClose();
        }

        protected virtual void OnOpen() => animator.SetTrigger(_openHashId);

        protected virtual void OnClose() => animator.SetTrigger(_closeHashId);

        protected abstract void OnBack();

        public virtual void Dispose() { }

        #region Animation Events.

        public void OnOpened()
        {
            OnOpenAnimationEnd?.Invoke();
            OnOpenAnimationEnd = null;
        }

        public void OnClosed()
        {
            gameObject.SetActive(false);

            OnCloseAnimationEnd?.Invoke();
            OnCloseAnimationEnd = null;
        }

        #endregion
    }
}
