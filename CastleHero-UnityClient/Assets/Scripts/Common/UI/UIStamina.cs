using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.Network.Service;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UIStamina : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _current;
        [SerializeField] private Slider _gauge;

        private Stamina _stamina;
        private bool _onUpdate;

        private void Awake()
        {
            this.SubscribeButton(_button, ShowLeftTime);
            _stamina = Storage.userRepository.stamina;
            _stamina.point
                .Subscribe(current =>
                {
                    int max = _stamina.pointLimit.Value;
                    _current.text = $"{current}/{max}";
                    _gauge.value = (float)current / max;
                })
                .AddTo(this);
        }

        private void ShowLeftTime()
        {
            var point = _stamina.point.Value;
            var pointLimit = _stamina.pointLimit.Value;
            var lastUpdate = _stamina.lastUpdate.Value;

            int minutesForNext = pointLimit < point
                ? 1
                : 0;

            int minutesForMax = Mathf.Max(0, pointLimit - point);

            var toNext =
                new ReactiveProperty<float>((float)(lastUpdate.AddMinutes(minutesForNext) - lastUpdate).TotalSeconds);

            var toMax =
                new ReactiveProperty<float>((float)(lastUpdate.AddMinutes(minutesForMax) - lastUpdate).TotalSeconds);

            string ToTimeText(float time)
            {
                return time > 0f
                    ? $"{(int)(time / 60):D2}:{(int)(time % 60):D2}"
                    : "-:-";
            }

            var toolTip = Context.toolTip;
            var subscription = Observable.Merge(
                    toNext,
                    toMax)
                .ThrottleFrame(1)
                .Subscribe(_ => { toolTip.Text = $"{ToTimeText(toNext.Value)} / {ToTimeText(toMax.Value)}"; });

            var update = Observable.EveryUpdate()
                .Select(_ => Time.deltaTime)
                .Subscribe(x =>
                {
                    toNext.Value = Mathf.Max(0, toNext.Value - x);
                    toMax.Value = Mathf.Max(0, toMax.Value - x);
                });
            
            toolTip.Open(string.Empty, transform as RectTransform, 0.5f, 1f);
            toolTip.onClosedQueue.Enqueue(() =>
            {
                subscription.Dispose();
                update.Dispose();
            });
        }
    }
}