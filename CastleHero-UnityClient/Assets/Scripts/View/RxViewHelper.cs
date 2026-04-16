using System;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;

namespace CastleHero.View
{
    /// <summary>
    /// View 레이어 전용 RxHelper 확장. PopupBase 의존성 + async 핸들러(Func&lt;UniTask&gt;) 오버로드 제공.
    /// </summary>
    public static class RxViewHelper
    {
        public static void AddTo<T>(this T disposable, PopupBase popup) where T : IDisposable
            => popup.DisposeOnClose(disposable);

        public static void SubscribeButton(this PopupBase popup, Button button, Action onClick,
            float clickThreshold = 0.25f)
        {
            SubscribeButton(popup, button, onClick, ServiceLocator.Get<SoundPath>().button, clickThreshold);
        }

        public static void SubscribeButton(this PopupBase popup, Button button, Action onClick,
            string clickSfx, float clickThreshold = 0.25f)
        {
            var subscription = RxHelper.ButtonSubscription(button, onClick, clickSfx, clickThreshold);
            popup.DisposeOnClose(subscription);
        }

        // async 버튼 핸들러: Func<UniTask> 를 받아 .Forget() 으로 fire-and-forget 실행.
        public static void SubscribeButton(this PopupBase popup, Button button, Func<UniTask> onClick,
            float clickThreshold = 0.25f)
            => popup.SubscribeButton(button, () => onClick().Forget(), clickThreshold);

        public static void SubscribeButton(this PopupBase popup, Button button, Func<UniTask> onClick,
            string clickSfx, float clickThreshold = 0.25f)
            => popup.SubscribeButton(button, () => onClick().Forget(), clickSfx, clickThreshold);

        public static void SubscribeButton(this MonoBehaviour behaviour, Button button, Action onClick,
            float clickThreshold = 0.25f)
        {
            behaviour.SubscribeButton(button, onClick, ServiceLocator.Get<SoundPath>().button, clickThreshold);
        }

        public static void SubscribeButton(this MonoBehaviour behaviour, Button button, Func<UniTask> onClick,
            float clickThreshold = 0.25f)
            => behaviour.SubscribeButton(button, () => onClick().Forget(), clickThreshold);
    }
}
