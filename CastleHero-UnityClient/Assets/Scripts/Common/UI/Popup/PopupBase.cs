using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Flow;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.Common.UI.Popup
{
    public abstract class PopupBase : MonoBehaviour, IBackButtonListener
    {
        public event Action<PopupBase> OnCloseEvent;

        private static readonly int CloseTrigger = Animator.StringToHash("Close");

        [SerializeField] private Animator _animator;
        [SerializeField] private Button _close;

        private void Awake() => OnAwake();

        protected virtual void OnAwake() => this.SubscribeButton(_close, () => CloseTask().Forget());

        public virtual UniTask Open(params object[] parameters)
        {
            return Open();
        }

        public virtual UniTask Open()
        {
            return UniTask.CompletedTask;
        }

        public UniTask CloseTask()
        {
            OnClose();

            return _animator == null ? DirectCloseTask() : CloseAnimationTask();
        }

        protected virtual void OnClose()
        {
        }

        private UniTask CloseAnimationTask() => TaskHelper.OnAnimationEnd(_animator, CloseTrigger, Closed);

        private UniTask DirectCloseTask()
        {
            Closed();
            return UniTask.CompletedTask;
        }

        public bool OnProcessBack()
        {
            CloseTask();

            return true;
        }

        private void Closed()
        {
            OnCloseEvent?.Invoke(this);
            Addressables.ReleaseInstance(gameObject);
        }
    }
}