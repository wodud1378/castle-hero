using System;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CastleHero.Common.Pattern;

using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.UI.Inventory.Popup
{
    public abstract class PopupItemBase<TSlot, TItem> : PopupBase
        where TSlot : UIItemSlot
        where TItem : class, IItem
    {
        [FormerlySerializedAs("_itemSlot")]
        [SerializeField] private TSlot itemSlot;
        [FormerlySerializedAs("_sell")]
        [SerializeField] private Button sell;

        public ItemEntity Entity { get; private set; }

        public TItem Item => item.Value;

        protected readonly ReactiveProperty<TItem> item = new();

        private bool _hasDataBefore;

        protected IDBProvider _db;
        protected IPopupManager _popups;
        protected IUserRepository _userRepo;
        protected INetworkServiceProvider _network;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _db = sl.Get<IDBProvider>();
            _popups = sl.Get<IPopupManager>();
            _userRepo = sl.Get<IUserRepository>();
            _network = sl.Get<INetworkServiceProvider>();

            SubscribeUpdate();

            item.Subscribe(OnDataChangedInternal)
                .AddTo(this);

            this.SubscribeButton(sell, Sell);
        }

        protected virtual void SubscribeUpdate()
        {
            _userRepo.Inventory
                .WhenUpdate(item, x => item.Value = x)
                .AddTo(this);
        }

        public override UniTask Open(params object[] parameters)
        {
            HandleParameters(parameters);

            return UniTask.CompletedTask;
        }

        protected virtual void HandleParameters(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
                return;

            if (parameters[0] is not TItem data)
                return;

            item.Value = data;
        }

        private void OnDataChangedInternal(TItem data)
        {
            if (data == null || data.Quantity == 0)
            {
                if (_hasDataBefore)
                    OnItemNullOrEmpty();

                return;
            }

            _hasDataBefore = true;
            OnDataChanged(data);
        }

        protected virtual void OnItemNullOrEmpty() => Close();

        protected virtual void OnDataChanged(TItem data)
        {
            if (!_db.Items.TryFind(data.ItemId, out var entity))
            {
                var exception = new Exception($"아이템을 찾을 수 없습니다. id={data.ItemId}");
                throw exception;
            }

            Entity = entity;
            sell.gameObject.SetActive(Entity.sellPrice > 0);
            InitSlot(item.Value, itemSlot);
        }

        protected virtual void InitSlot(TItem item, TSlot slot) => slot.Init(item, Entity);

        private async UniTask Sell()
        {
            if (Entity.sellPrice <= 0)
                return;

            var items = new IItem[] { Item };
            var quantities = new[] { SellCount };

            var price = Entity.sellPrice * SellCount;
            string popupText = $"{price:N0} 골드에 판매하시겠습니까?";
            var selectSource = new UniTaskCompletionSource<bool>();
            var param = new PopupCommon.ButtonParam[]
            {
                new()
                {
                    action = PopupCommon.ButtonAction.Confirm,
                    onClick = () => selectSource.TrySetResult(true)
                },
                new()
                {
                    action = PopupCommon.ButtonAction.Cancel,
                    onClick = () => selectSource.TrySetResult(false)
                }
            };

            _popups.Open<PopupCommon>(popupText, param);

            if (!await selectSource.Task)
                return;

            await _network.Inventory.Sell(items, quantities);
        }

        protected abstract int SellCount { get; }
    }
}
