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

            if (point >= pointLimit)
                return;

            int minutesForNext = Stamina.INTERVAL;
            int minutesForMax = (pointLimit - point) / Stamina.PER_ONCE * Stamina.INTERVAL;
            var now = NetworkService.CurrentTimeByLocal();
            var toNext = (float)(lastUpdate.AddMinutes(minutesForNext) - now).TotalSeconds;
            var toMax = (float)(lastUpdate.AddMinutes(minutesForMax) - now).TotalSeconds;

            string ToTimeText(float time) => $"{(int)(time / 60):D2}:{(int)(time % 60):D2}";

            var toolTip = Context.toolTip;
            var update = Observable.EveryUpdate()
                .Select(_ => Time.deltaTime)
                .Subscribe(x =>
                {
                    toNext = Mathf.Max(0, toNext - x);
                    toMax = Mathf.Max(0, toMax - x);
                    toolTip.Text = $"{ToTimeText(toNext)} / {ToTimeText(toMax)}";
                });

            toolTip.Open(string.Empty, transform as RectTransform, 0.5f, 1f);
            toolTip.onClosedQueue.Enqueue(() => update.Dispose());
        }
    }
}