using System;
using RGLabs.Common.UI;
using UnityEngine;
using UnityEngine.U2D;

namespace RGLabs.Common.Behaviours
{
    public abstract class UIMain : MonoBehaviour, IDisposable
    {
        public event Action OnOpenAnimationEnd;
        public event Action OnCloseAnimationEnd;
        
        public bool IsOpen { get; private set; }

        [SerializeField] private UIAtlasedSpriteCollection _spriteCollection;
        [SerializeField] private Animator _animator;
        
        private readonly int _openHashId = Animator.StringToHash("Entrance");
        private readonly int _closeHashId = Animator.StringToHash("Exit");

        public void Open()
        {
            IsOpen = true;
            
            gameObject.SetActive(true);
            
            _animator.SetTrigger(_openHashId);
        }

        public void Close()
        {
            IsOpen = false;
            
            _animator.SetTrigger(_closeHashId);
        }

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
            _spriteCollection.Dispose();
            
            gameObject.SetActive(false);
            
            OnCloseAnimationEnd?.Invoke();
            OnCloseAnimationEnd = null;
        }

        #endregion
    }
}