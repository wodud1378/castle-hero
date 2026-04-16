using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Network.Service.Boot;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.Title.UI.Popup
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

        [FormerlySerializedAs("_all")]
        [SerializeField] private Toggle all;
        [FormerlySerializedAs("_essentials")]
        [SerializeField] private List<Toggle> essentials;
        [FormerlySerializedAs("_optionals")]
        [SerializeField] private List<Toggle> optionals;
        [FormerlySerializedAs("_confirm")]
        [SerializeField] private Button confirm;
        [FormerlySerializedAs("_needEssential")]
        [SerializeField] private CanvasGroup needEssential;

        public UniTask<PolicyAgreement> AgreementTask => _completionSource.Task;

        private UniTaskCompletionSource<PolicyAgreement> _completionSource;

        protected override void OnAwake()
        {
            base.OnAwake();

            all.onValueChanged
                .AsObservable()
                .Subscribe(x =>
                {
                    if (x)
                    {
                        essentials.ForEach(t => t.isOn = true);
                        optionals.ForEach(t => t.isOn = true);
                    }

                    all.interactable = !x;
                })
                .AddTo(this);

            Observable.CombineLatest(
                    OnToggleChanged(Essential.Terms),
                    OnToggleChanged(Essential.Privacy),
                    OnToggleChanged(Optional.Notification))
                .ThrottleFrame(1)
                .Subscribe(values =>
                {
                    bool allOn = values.All(x => x);
                    all.isOn = allOn;
                })
                .AddTo(this);

            this.SubscribeButton(confirm, OnConfirm);
        }

        public override UniTask Open()
        {
            _completionSource = new();

            return UniTask.CompletedTask;
        }

        private void OnConfirm()
        {
            if (essentials.Any(x => !x.isOn))
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

        private Toggle EssentialToggle(Essential type) => essentials[(int)type];

        private Toggle OptionalToggle(Optional type) => optionals[(int)type];

        private IObservable<bool> OnToggleChanged(Essential type)
            => EssentialToggle(type).onValueChanged.AsObservable();

        private IObservable<bool> OnToggleChanged(Optional type)
            => OptionalToggle(type).onValueChanged.AsObservable();

        private void ShowNeedEssential()
        {
            // DOTween Free의 CanvasGroup.DOFade 는 Modules 의존이라 asmdef 경계에서 미노출.
            // DOTween.To 코어 API로 대체.
            DOTween.Sequence()
                .Append(DOTween.To(() => needEssential.alpha, v => needEssential.alpha = v, 1f, 0.15f).From(0f))
                .AppendInterval(1f)
                .Append(DOTween.To(() => needEssential.alpha, v => needEssential.alpha = v, 0f, 0.15f).From(1f))
                .Play();
        }
    }
}
