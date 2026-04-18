using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.Data;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using CastleHero.Common.Sound;
using CastleHero.Common.Pattern;

namespace CastleHero.View.Common.UI.Popup
{
    public interface ISelect<T>
    {
        public UniTask<T> SelectTask { get; }
        public void BeginSelect(bool closeAfterSelect);
    }
    
    public abstract class PopupBase : MonoBehaviour, IBackButtonListener
    {
        public event Action<PopupBase> OnCloseEvent;

        public bool fromManager = true;

        private static readonly int OpenTrigger = Animator.StringToHash("Open");
        private static readonly int CloseTrigger = Animator.StringToHash("Close");

        [SerializeField] protected Animator _animator;
        [SerializeField] protected Button _close;

        private readonly List<IDisposable> _disposables = new();
        private bool _onClose;

        private ISoundManager _sounds;
        private SoundPath _soundPath;

        private void Awake()
        {
            var sl = ServiceLocator.Instance;
            _sounds = sl.Get<ISoundManager>();
            _soundPath = sl.Get<SoundPath>();

            _onClose = false;

            OnAwake();
        }

        protected virtual void OnAwake()
        {
            if(_close != null)
                this.SubscribeButton(_close, Close);
            
            if (_animator == null)
                _animator = GetComponent<Animator>();
            
            if(HasTrigger(OpenTrigger))
                _animator.SetTrigger(OpenTrigger);
        }

        public virtual UniTask Open(params object[] parameters) => Open();

        public virtual UniTask Open() => UniTask.CompletedTask;

        public void DisposeOnClose(IDisposable disposable)
        {
            if (_onClose)
            {
                disposable.Dispose();
                return;
            }
            
            _disposables.Add(disposable);
        }

        private void OnEnable() => PlaySfx(_soundPath?.openPopup);

        private void OnDisable() => PlaySfx(_soundPath?.closePopup);

        protected void PlaySfx(string sfx)
        {
            if (_sounds == null)
                return;

            _sounds.PlaySfx(sfx);
        }

        public async UniTask CloseAsync()
        {
            OnClose();

            if (HasTrigger(CloseTrigger))
                await CloseAnimationTask();
            
            Closed();
        }

        public void Close() => CloseAsync().SafeForget();

        protected virtual void OnClose()
        {
            _onClose = true;
            
            _disposables.ForEach(x => x.Dispose());
            _disposables.Clear();
        }

        private UniTask CloseAnimationTask() => TaskHelper.OnAnimationEnd(_animator, CloseTrigger);

        public bool OnProcessBack()
        {
            CloseAsync().SafeForget();

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