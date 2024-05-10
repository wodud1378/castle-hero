using System;
using RGLabs.Common.UI;
using UnityEngine;

namespace RGLabs.Common.Behaviours
{
    public abstract class UIMain : MonoBehaviour
    {
        public event Action OnOpenAnimationEnd;
        public event Action OnCloseAnimationEnd;
        
        public bool IsOpen { get; private set; }

        [SerializeField] private UIAtlasedSpriteCollection _atlasCollections;
        [SerializeField] private Animator _animator;
        
        private readonly int _openHashId = Animator.StringToHash("Entrance");
        private readonly int _closeHashId = Animator.StringToHash("Exit");

        public void Open()
        {
            _atlasCollections.enabled = true;
            
            IsOpen = true;
            
            _animator.SetTrigger(_openHashId);
        }

        public void Close()
        {
            IsOpen = false;
            
            _animator.SetTrigger(_closeHashId);
        }

        #region Animation Events.

        public void OnOpened()
        {
            OnOpenAnimationEnd?.Invoke();

            OnOpenAnimationEnd = null;
        }

        public void OnClosed()
        {
            OnCloseAnimationEnd?.Invoke();

            OnCloseAnimationEnd = null;

            _atlasCollections.enabled = false;
        }

        #endregion
    }
}