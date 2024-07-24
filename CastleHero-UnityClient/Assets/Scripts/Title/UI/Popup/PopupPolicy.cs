using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.UI.Popup;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Title.UI.Popup
{
    public class PopupPolicy : PopupBase
    {
        [SerializeField] private Toggle _all;
        [SerializeField] private Toggle _terms;
        [SerializeField] private Toggle _privacy;
        [SerializeField] private Toggle _push;
        [SerializeField] private Toggle _nightPush;
        [SerializeField] private Button _confirm;
        [SerializeField] private CanvasGroup _needEssential;

        public UniTask<PolicyAgreement> AgreementTask => _completionSource.Task;
        
        private UniTaskCompletionSource<PolicyAgreement> _completionSource;
        private Sequence _sequence;

        protected override void OnAwake()
        {
            base.OnAwake();

            _all.onValueChanged
                .AsObservable()
                .Subscribe(x =>
                {
                    if (x)
                    {
                        _terms.isOn = true;
                        _privacy.isOn = true;
                        _push.isOn = true;
                        _nightPush.isOn = true; 
                    }
                })
                .AddTo(this);

            Observable.CombineLatest(
                    _terms.onValueChanged.AsObservable(),
                    _privacy.onValueChanged.AsObservable(),
                    _push.onValueChanged.AsObservable(),
                    _nightPush.onValueChanged.AsObservable())
                .ThrottleFrame(1)
                .Subscribe(values =>
                {
                    bool all = values.All(x => x);
                    _all.isOn = all;
                })
                .AddTo(this);
            
            this.SubscribeButton(_confirm, OnConfirm);

            _sequence = DOTween.Sequence()
                .Append(_needEssential.DOFade(1f, 0.15f).From(0f))
                .AppendInterval(1f)
                .Append(_needEssential.DOFade(0f, 0.15f).From(1f));
        }

        public override UniTask Open()
        {
            _completionSource = new();
            
            return UniTask.CompletedTask;
        }

        private void OnConfirm()
        {
            if (!_terms.isOn || !_privacy.isOn)
            {
                ShowNeedEssential();
                return;
            }
            
            var agreement = new PolicyAgreement
            {
                terms = _terms.isOn,
                privacy = _privacy.isOn,
                push = _push.isOn,
                nightPush = _nightPush.isOn
            };

            _completionSource.TrySetResult(agreement);
            
            CloseAsync().Forget();
        }

        private void ShowNeedEssential()
        {
            _sequence.Kill();
            _sequence.Play();
        }
    }
}