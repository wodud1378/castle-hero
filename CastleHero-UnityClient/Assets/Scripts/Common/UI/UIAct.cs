using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UIAct : MonoBehaviour
    {
        private struct Calculation
        {
            public int point;
            public int pointLimit;
            public DateTime lastUpdate;
        }

        private static UIAct _attached;

        private const int ApAddIntervalMinute = 10;
        private const int ApAddPerOnce = 1;

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _current;
        [SerializeField] private Slider _gauge;

        private Act _act;
        private bool _onUpdate;

        private void Awake()
        {
            this.SubscribeButton(_button, ShowLeftTime);
            _act = Storage.userRepository.act;
            if (_attached == null)
            {
                Attach();
            }

            _act.point
                .Subscribe(current =>
                {
                    int max = _act.pointLimit.Value;
                    _current.text = $"{current}/{max}";
                    _gauge.value = (float)current / max;
                })
                .AddTo(this);
        }

        private void ShowLeftTime()
        {
            var point = _act.point.Value;
            var pointLimit = _act.pointLimit.Value;
            var lastUpdate = _act.lastUpdate.Value;

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

        private void Attach()
        {
            UpdateTask().Forget();
        }

        private async UniTaskVoid UpdateTask()
        {
            while (_onUpdate)
            {
                // 계산은 스레드 풀에서 진행.
                var result = await UniTask.RunOnThreadPool(Calculate);

                await UniTask.SwitchToMainThread();

                // UI 갱신 가능성이 있는 스트림은 메인 스레드에서 업데이트.
                _act.point.Value = result.point;
                _act.pointLimit.Value = result.pointLimit;
                _act.lastUpdate.Value = result.lastUpdate;
            }
        }

        private DateTime CurrentTime() => DateTime.UtcNow.AddHours(3);

        private Calculation Calculate()
        {
            var now = CurrentTime();
            int limit = _act.pointLimit.Value;
            var calculation = new Calculation
            {
                pointLimit = limit,
                lastUpdate = now
            };

            int point = _act.point.Value;
            if (point >= limit)
            {
                calculation.point = point;
            }
            else
            {
                var minutes = (now - _act.lastUpdate.Value).Minutes;
                int amount = minutes / ApAddIntervalMinute * ApAddPerOnce;
                int total = point + amount;
                calculation.point = total < limit ? total : limit;
            }

            return calculation;
        }

        private void OnDestroy()
        {
            if (_attached != this)
                return;

            _attached = null;
            _onUpdate = false;
        }
    }
}