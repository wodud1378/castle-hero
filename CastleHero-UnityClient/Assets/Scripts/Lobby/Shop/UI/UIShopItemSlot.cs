using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopItemSlot : UISlot
    {
        public UISlot price;
        public TMP_Text leftTime;
        public TMP_Text leftCount;

        public readonly ReactiveProperty<ShopItemEntity> data = new();
        public readonly ReactiveProperty<Product> product = new();

        private readonly Timer _timer = new();
        private IDisposable _timerSubscription;

        private void Awake()
        {
            Observable.Merge(
                    data.Select(_ => UniRx.Unit.Default),
                    product.Select(_ => UniRx.Unit.Default))
                .ThrottleFrame(1)
                .Subscribe(_ => UpdateUI())
                .AddTo(this);
        }

        public UniTask Init(ShopItemEntity entity)
        {
            data.Value = entity;
            product.Value = Storage.userRepository.shopRecord.products
                .FirstOrDefault(x => x.shopId == entity.Id);

            return base.Init(string.Empty, entity.name);
        }

        private void UpdateUI()
        {
            var entity = data.Value;
            if (!entity.IsValid)
                return;

            if (leftCount != null)
            {
                int limit = entity.count;
                leftCount.text = product.Value == null
                    ? $"{limit}/{limit}"
                    : $"{limit - product.Value.byDefault}/{limit}";
            }

            string spritePath = entity.costId switch
            {
                1 or 2 => Constants.DiaIcon,
                3 => Constants.GoldIcon,
                _ => string.Empty
            };

            string text = entity.costId == 0
                ? $"\uffe6 {entity.costValue:N0}"
                : $"{entity.costValue:N0}";

            price.Init(spritePath, text)
                .Forget();

            SetTimer();
        }

        private void SetTimer()
        {
            if (leftTime == null)
                return;
            
            var entity = data.Value;
            if (entity.endDate == default)
            {
                leftTime.gameObject.SetActive(false);
                return;
            }

            leftTime.gameObject.SetActive(true);

            var currentTime = NetworkService.CurrentTime();
            var endTime = entity.endDate;
            var seconds = (endTime - currentTime).TotalSeconds;

            _timer.Run(seconds);
            _timerSubscription = _timer.leftTime
                .Subscribe(OnTimerUpdate);
        }

        private void OnTimerUpdate(double seconds)
        {
            if (seconds > 0)
            {
                leftTime.text = seconds.ToLeftTimeText();
                return;
            }

            _timerSubscription.Dispose();
            SetTimer();
        }

        private void OnDestroy()
        {
            _timerSubscription?.Dispose();
            _timer.Dispose();
        }
    }
}