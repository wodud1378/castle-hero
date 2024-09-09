using System;
using System.Collections.Generic;
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
        public enum Essential
        {
            Terms = 0,
            Privacy,
        }

        public enum Optional
        {
            Notification,
            NightNotification,
        }

        [SerializeField] private Toggle _all;
        [SerializeField] private List<Toggle> _essentials;
        [SerializeField] private List<Toggle> _optionals;
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
                        _essentials.ForEach(t => t.isOn = true);
                        _optionals.ForEach(t => t.isOn = true);    
                    }

                    _all.interactable = !x;
                })
                .AddTo(this);

            var a = _essentials[0].onValueChanged.AsObservable();

            Observable.CombineLatest(
                    OnToggleChanged(Essential.Terms),
                    OnToggleChanged(Essential.Privacy),
                    OnToggleChanged(Optional.Notification))
                .ThrottleFrame(1)
                .Subscribe(values =>
                {
                    bool all = values.All(x => x);
                    _all.isOn = all;
                })
                .AddTo(this);

            this.SubscribeButton(_confirm, OnConfirm);
        }

        public override UniTask Open()
        {
            _completionSource = new();

            return UniTask.CompletedTask;
        }

        private void OnConfirm()
        {
            if (_essentials.Any(x => !x.isOn))
            {
                ShowNeedEssential();
                return;
            }

            var agreement = new PolicyAgreement
            {
                terms = EssentialToggle(Essential.Terms).isOn,
                privacy = EssentialToggle(Essential.Privacy).isOn,
                push = OptionalToggle(Optional.Notification).isOn,
                nightPush = OptionalToggle(Optional.NightNotification).isOn,
                updated = true,
            };

            _completionSource.TrySetResult(agreement);

            CloseAsync().Forget();
        }

        private Toggle EssentialToggle(Essential type) => _essentials[(int)type];

        private Toggle OptionalToggle(Optional type) => _optionals[(int)type];

        private IObservable<bool> OnToggleChanged(Essential type)
            => EssentialToggle(type).onValueChanged.AsObservable();

        private IObservable<bool> OnToggleChanged(Optional type)
            => OptionalToggle(type).onValueChanged.AsObservable();

        private void ShowNeedEssential()
        {
            _sequence ??= DOTween.Sequence()
                .Append(_needEssential.DOFade(1f, 0.15f).From(0f))
                .AppendInterval(1f)
                .Append(_needEssential.DOFade(0f, 0.15f).From(1f));
            
            _sequence.Kill();
            _sequence.Play();
        }
    }
}