using System;
using System.Collections.Generic;
using System.Linq;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.View.Lobby.Shop.Popup;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using TMPro;
using UniRx;
using CastleHero.Common.Pattern;

using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.Shop.UI
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

        private IUserRepository _userRepo;
        private IPopupManager _popups;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _userRepo = sl.Get<IUserRepository>();
            _popups = sl.Get<IPopupManager>();

            Observable.Merge(
                    data.Select(_ => UniRx.Unit.Default),
                    product.Select(_ => UniRx.Unit.Default))
                .ThrottleFrame(1)
                .Subscribe(_ => UpdateUI())
                .AddTo(this);

            _userRepo.ShopRecord.Products
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

        public void Init(ShopItemEntity entity)
        {
            data.Value = entity;
            product.Value = _userRepo.ShopRecord.Products
                .FirstOrDefault(x => x.shopId == entity.Id);

            base.Init(string.Empty, entity.name);
        }

        private void UpdateUI()
        {
            var entity = data.Value;
            if (!entity.IsValid)
                return;
            
            ShopHelper.GetBuyCount(entity, product.Value, out int left, out int limit);
            if (leftCount != null)
            {
                leftCount.text = left == 0 && limit == 0
                    ? string.Empty
                    : $"{left}/{limit}";
            }
            
            _isSoldOut = ShopHelper.IsSoldOut(data.Value, product.Value, out _, out _, out _);
            
            price.Init(entity, product.Value);

            state.Value = _isSoldOut
                ? State.Dim
                : State.Default;

            price.gameObject.SetActive(!_isSoldOut);

            SetSchedule();
        }

        private void OnClickSlot(UISlot _) => _popups.Open<PopupPurchase>(data.Value, product.Value);

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

            var currentTime = ServerTime.Now;
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

            var currentTime = ServerTime.Now;
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