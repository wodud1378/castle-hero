using Cysharp.Threading.Tasks;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CastleHero.View.Common.UI.Popup
{
    public abstract class PopupLeftToRight<TData, TSlot> : PopupBase where TSlot : UISlot
    {
        [FormerlySerializedAs("_left")]
        [SerializeField] private TSlot leftSlot;
        [FormerlySerializedAs("_right")]
        [SerializeField] private TSlot rightSlot;
        [FormerlySerializedAs("_submit")]
        [SerializeField] private Button submit;

        protected readonly ReactiveProperty<TData> left = new();
        protected readonly ReactiveProperty<TData> right = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            Observable.Merge(left, right)
                .ThrottleFrame(1)
                .Subscribe()
                .AddTo(this);

            this.SubscribeButton(submit, OnSubmit);
        }

        public override UniTask Open(params object[] parameters)
        {
            HandleParameters(parameters);

            UpdateUI();
            return UniTask.CompletedTask;
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
            InitSlot(left.Value, leftSlot);
            InitSlot(right.Value, rightSlot);
        }

        protected abstract void InitSlot(TData data, TSlot slot);

        protected abstract void OnSubmit();
    }
}
