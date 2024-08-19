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
using UnityEngine;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopItemSlot : UISlot
    {
        [SerializeField] private TMP_Text _leftTime;
        [SerializeField] private TMP_Text _leftCount;

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
                .Subscribe(_=> UpdateUI())
                .AddTo(this);
        }

        public UniTask Init(ShopItemEntity entity)
        {
            data.Value = entity;
            product.Value = Storage.userRepository.shopRecord.products
                .FirstOrDefault(x => x.shopId == entity.Id);
            
            return base.Init(entity.image, entity.name);
        }

        private void UpdateUI()
        {
            var entity = data.Value;
            if (!entity.IsValid)
                return;

            int limit = entity.count;
            _leftCount.text = product == null
                ? $"{limit}/{limit}"
                : $"{limit - product.Value.byDefault}/{limit}";
            
            SetTimer();
        }

        private void SetTimer()
        {
            var entity = data.Value;
            if (entity.endDate == default)
            {
                _leftTime.gameObject.SetActive(false);
                return;
            }
            
            _leftTime.gameObject.SetActive(true);
                
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
                _leftTime.text = seconds.ToLeftTimeText();
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