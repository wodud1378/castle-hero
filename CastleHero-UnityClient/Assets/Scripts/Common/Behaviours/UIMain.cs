using System;
using RGLabs.Common.Sound;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace RGLabs.Common.Behaviours
{
    public abstract class UIMain : MonoBehaviour, IDisposable
    {
        public event Action OnOpenAnimationEnd;
        public event Action OnCloseAnimationEnd;
        
        public bool IsOpen { get; private set; }

        [SerializeField] private UIAtlasedSpriteCollection _spriteCollection;
        [SerializeField] private Animator _animator;
        [SerializeField] private Button _back;
        
        private readonly int _openHashId = Animator.StringToHash("Entrance");
        private readonly int _closeHashId = Animator.StringToHash("Exit");

        private void Awake()
        {
            if(_back != null)
                this.SubscribeButton(_back, OnBack, Storage.soundPath.back);
        }

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

        protected virtual void OnOpen() => _animator.SetTrigger(_openHashId);

        protected virtual void OnClose() => _animator.SetTrigger(_closeHashId);

        protected abstract void OnBack();

        public virtual void Dispose()
        {
            _spriteCollection.Dispose();
        }

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