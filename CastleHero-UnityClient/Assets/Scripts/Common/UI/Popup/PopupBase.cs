using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.Common.UI.Popup
{
    public abstract class PopupBase : MonoBehaviour, IBackButtonListener
    {
        public event Action<PopupBase> OnCloseEvent;

        public bool fromManager = true;

        private static readonly int OpenTrigger = Animator.StringToHash("Open");
        private static readonly int CloseTrigger = Animator.StringToHash("Close");

        [SerializeField] protected Animator _animator;
        [SerializeField] protected Button _close;

        private void Awake() => OnAwake();

        protected virtual void OnAwake()
        {
            if(_close != null)
                this.SubscribeButton(_close, () => CloseAsync().Forget());
            
            if (_animator == null)
                _animator = GetComponent<Animator>();
            
            if(HasTrigger(OpenTrigger))
                _animator.SetTrigger(OpenTrigger);
        }

        public virtual UniTask Open(params object[] parameters) => Open();

        public virtual UniTask Open() => UniTask.CompletedTask;

        private void OnEnable() => PlaySfx(Storage.soundPath.openPopup);

        private void OnDisable() => PlaySfx(Storage.soundPath.closePopup);

        protected void PlaySfx(string sfx)
        {
            if (Context.sounds == null)
                return;
            
            Context.sounds.PlaySfx(sfx);
        }

        public async UniTask CloseAsync()
        {
            OnClose();

            if (HasTrigger(CloseTrigger))
                await CloseAnimationTask();
            
            Closed();
        }

        protected virtual void OnClose()
        {
        }

        private UniTask CloseAnimationTask() => TaskHelper.OnAnimationEnd(_animator, CloseTrigger);

        public bool OnProcessBack()
        {
            CloseAsync().Forget();

            return true;
        }

        private bool HasTrigger(int hash)
        {
            return _animator != null && _animator.parameters
                .Where(x => x.type == AnimatorControllerParameterType.Trigger)
                .Any(x => x.nameHash == hash);
        }

        private void Closed()
        {
            OnCloseEvent?.Invoke(this);
            
            if(fromManager)
                Addressables.ReleaseInstance(gameObject);
            else
                gameObject.SetActive(false);
        }

        private void OnValidate()
        {
            if(_animator == null)
                _animator = GetComponent<Animator>();
        }
    }
}