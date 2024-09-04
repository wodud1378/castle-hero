using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI.Popup
{
    public abstract class PopupLeftToRight<TData, TSlot> : PopupBase where TSlot : UISlot
    {
        [SerializeField] private TSlot _left;
        [SerializeField] private TSlot _right;
        [SerializeField] private Button _submit;

        protected readonly ReactiveProperty<TData> left = new();
        protected readonly ReactiveProperty<TData> right = new();

        private UniTask _updateTask;

        protected override void OnAwake()
        {
            base.OnAwake();

            Observable.Merge(left, right)
                .ThrottleFrame(1)
                .Subscribe()
                .AddTo(this);

            this.SubscribeButton(_submit, OnSubmit);
        }

        public override UniTask Open(params object[] parameters)
        {
            HandleParameters(parameters);

            UpdateUI();
            return _updateTask;
        }

        protected virtual void HandleParameters(params object[] parameters)
        {
            if (parameters[0] is TData l)
                left.Value = l;

            if (parameters[1] is TData r)
                right.Value = r;
        }

        protected virtual void UpdateUI()
        {
            _updateTask = UniTask.WhenAll(
                InitSlot(left.Value, _left),
                InitSlot(right.Value, _right));
        }

        protected abstract UniTask InitSlot(TData data, TSlot slot);

        protected abstract void OnSubmit();
    }
}