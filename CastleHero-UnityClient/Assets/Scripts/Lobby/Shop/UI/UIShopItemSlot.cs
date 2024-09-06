using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.Shop.Popup;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopItemSlot : UISlot
    {
        public UIPrice price;
        public TMP_Text leftTimeForReset;
        public TMP_Text leftTimeForExpire;
        public TMP_Text leftCount;

        public readonly ReactiveProperty<ShopItemEntity> data = new();
        public readonly ReactiveProperty<Product> product = new();

        private readonly Timer _resetTimer = new();
        private readonly Timer _expireTimer = new();

        private IDisposable _resetSubscription;
        private IDisposable _expireSubscription;

        private bool _isSoldOut;

        protected override void OnAwake()
        {
            base.OnAwake();

            Observable.Merge(
                    data.Select(_ => UniRx.Unit.Default),
                    product.Select(_ => UniRx.Unit.Default))
                .ThrottleFrame(1)
                .Subscribe(_ => UpdateUI())
                .AddTo(this);

            Storage.userRepository.shopRecord.products
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x =>
                {
                    if (!data.Value.IsValid)
                        return;

                    product.Value = x.FirstOrDefault(p => p.shopId == data.Value.Id);
                })
                .AddTo(this);

            OnClick += OnClickSlot;
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

            var paymentType = ShopHelper.GetPaymentType(data.Value, product.Value, out int left, out int limit);
            if (leftCount != null)
            {
                leftCount.text = left == 0 && limit == 0
                    ? string.Empty
                    : $"{left}/{limit}";
            }

            price.Init(paymentType, entity.costId, entity.costValue)
                .Forget();

            _isSoldOut = ShopHelper.IsSoldOut(data.Value, product.Value, out _, out _, out _);

            state.Value = _isSoldOut
                ? State.Dim
                : State.Default;

            price.gameObject.SetActive(!_isSoldOut);

            SetSchedule();
        }

        private void OnClickSlot(UISlot _)
        {
            var paymentType = ShopHelper.GetPaymentType(data.Value, product.Value, out int _, out int _);

            Context.popups.Open<PopupPurchase>(paymentType, data.Value);
        }

        private void SetSchedule()
        {
            if (leftTimeForReset != null)
                leftTimeForReset.gameObject.SetActive(false);
            
            if (leftTimeForExpire != null)
                leftTimeForExpire.gameObject.SetActive(false);

            if (product.Value == null)
                return;

            SetResetSchedule();
            SetExpireSchedule();
        }

        private void SetResetSchedule()
        {
            if (leftTimeForReset == null)
                return;

            _resetSubscription?.Dispose();
            _resetSubscription = null;

            var currentTime = NetworkService.CurrentTimeByLocal();
            var nextReset = product.Value.nextReset;
            if (_isSoldOut && nextReset != DateTime.MinValue)
            {
                leftTimeForReset.gameObject.SetActive(true);
                _resetTimer.Run((nextReset - currentTime).TotalSeconds);
                _resetSubscription =
                    _resetTimer.leftTime.Subscribe(s => leftTimeForReset.text = s.ToLeftTimeForResetText());

                _resetTimer.OnFinished -= SetResetSchedule;
                _resetTimer.OnFinished += SetResetSchedule;
            }
            else
            {
                leftTimeForReset.gameObject.SetActive(false);
            }
        }

        private void SetExpireSchedule()
        {
            if (leftTimeForExpire == null)
                return;

            _expireSubscription?.Dispose();
            _expireSubscription = null;

            var currentTime = NetworkService.CurrentTimeByLocal();
            var expire = product.Value.expireDate;
            if (expire != DateTime.MinValue)
            {
                leftTimeForExpire.gameObject.SetActive(true);
                _expireTimer.Run((expire - currentTime).TotalSeconds);
                _expireSubscription = _expireTimer.leftTime.Subscribe(
                    s => leftTimeForExpire.text = s.ToLeftTimeForExpireText());

                _expireTimer.OnFinished -= SetExpireSchedule;
                _expireTimer.OnFinished += SetExpireSchedule;
            }
            else
            {
                leftTimeForExpire.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _expireTimer.Dispose();
            _resetTimer.Dispose();
        }
    }
}