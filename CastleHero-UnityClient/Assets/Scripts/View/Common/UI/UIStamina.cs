using System;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Data;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CastleHero.View.Common.UI;
using CastleHero.Common.Pattern;

namespace CastleHero.View.Common.UI
{
    public class UIStamina : MonoBehaviour
    {
        [FormerlySerializedAs("_button")]
        [SerializeField] private Button button;
        [FormerlySerializedAs("_current")]
        [SerializeField] private TMP_Text current;
        [FormerlySerializedAs("_gauge")]
        [SerializeField] private Slider gauge;

        private Stamina _stamina;
        private bool _onUpdate;

        private void Awake()
        {
            this.SubscribeButton(button, ShowLeftTime);
            _stamina = ServiceLocator.Get<IUserRepository>().Stamina;
            _stamina.Point
                .Subscribe(currentVal =>
                {
                    int max = _stamina.PointLimit.Value;
                    current.text = $"{currentVal}/{max}";
                    gauge.value = (float)currentVal / max;
                })
                .AddTo(this);
        }

        private void ShowLeftTime()
        {
            var point = _stamina.Point.Value;
            var pointLimit = _stamina.PointLimit.Value;
            var lastUpdate = _stamina.LastUpdate.Value;

            if (point >= pointLimit)
                return;

            int minutesForNext = Stamina.INTERVAL;
            int minutesForMax = (pointLimit - point) / Stamina.PER_ONCE * Stamina.INTERVAL;
            var now = ServerTime.Now;
            var toNext = (float)(lastUpdate.AddMinutes(minutesForNext) - now).TotalSeconds;
            var toMax = (float)(lastUpdate.AddMinutes(minutesForMax) - now).TotalSeconds;

            string ToTimeText(float time) => $"{(int)(time / 60):D2}:{(int)(time % 60):D2}";

            var toolTip = ServiceLocator.Get<UIToolTip>();
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
