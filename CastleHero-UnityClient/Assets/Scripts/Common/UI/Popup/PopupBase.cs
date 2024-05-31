using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Flow;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.Common.UI.Popup
{
    public abstract class PopupBase : MonoBehaviour, IBackButtonListener
    {
        public event Action<PopupBase> OnClose;

        private static readonly int CloseTrigger = Animator.StringToHash("Close");
        
        [SerializeField] private Animator _animator;
        [SerializeField] private Button _close;

        private void Awake()
        {
            _close
                .OnClickAsObservable()
                .Subscribe(_ => Close())
                .AddTo(this);
        }

        public virtual UniTask Open()
        {
            gameObject.SetActive(true);

            return UniTask.DelayFrame(1);
        }

        public UniTask Close() => _animator == null ? DirectCloseTask() : CloseAnimationTask();

        private UniTask CloseAnimationTask()
        {
            _animator.SetTrigger(CloseTrigger);
            
            var task = Observable.EveryUpdate()
                .Where(_ =>
                {
                    var state = _animator.GetCurrentAnimatorStateInfo(0);
                    return state.shortNameHash == CloseTrigger && state.normalizedTime >= 1f;
                })
                .ToUniTask()
                .ContinueWith(_=> Closed());
            
            return task;
        }

        private UniTask DirectCloseTask()
        {
            Closed();
            return UniTask.CompletedTask;
        }
        
        public bool OnProcessBack()
        {
            Close();
            
            return true;
        }
        
        private void Closed()
        {
            OnClose?.Invoke(this);
            Addressables.ReleaseInstance(gameObject);
        }
    }
}