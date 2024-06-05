using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Utility;
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
            gameObject.SetActive(true);
            
            IsOpen = true;
            
            TaskHelper.OnAnimationEnd(_animator, _openHashId, OnOpened).Forget();
        }

        public void Close()
        {
            IsOpen = false;
            
            TaskHelper.OnAnimationEnd(_animator, _closeHashId, OnClosed).Forget();
        }

        public virtual void Dispose()
        {
            _spriteCollection.Dispose();
        }

        public void OnOpened()
        {
            OnOpenAnimationEnd?.Invoke();
            OnOpenAnimationEnd = null;
        }

        public void OnClosed()
        {
            _spriteCollection.Dispose();
            
            OnCloseAnimationEnd?.Invoke();
            
            gameObject.SetActive(false);
        }
    }
}