using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    public abstract class PopupItemBase<TSlot, TItem> : PopupBase
        where TSlot : UIItemSlot
        where TItem : class, IItem
    {
        [SerializeField] private TSlot _itemSlot;
        [SerializeField] private Button _sell;

        public ItemEntity Entity { get; private set; }

        public TItem Item => item.Value;

        protected readonly ReactiveProperty<TItem> item = new();

        private UniTask _updateTask;
        private bool _hasDataBefore;
        
        protected override void OnAwake()
        {
            base.OnAwake();

            SubscribeUpdate();
            
            item.Subscribe(OnDataChangedInternal)
                .AddTo(this);
            
            this.SubscribeButton(_sell, Sell);
        }

        protected virtual void SubscribeUpdate()
        {
            Storage.userRepository.inventory
                .WhenUpdate(item, x => item.Value = x)
                .AddTo(this);
        }
        
        public override UniTask Open(params object[] parameters)
        {
            HandleParameters(parameters);
            
            return _updateTask;
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
            if (!Storage.db.items.TryFind(data.ItemId, out var entity))
            {
                var exception = new Exception($"아이템을 찾을 수 없습니다. id={data.ItemId}");
                throw exception;
            }

            Entity = entity;
            _sell.gameObject.SetActive(Entity.sellPrice > 0);
            _updateTask = InitSlot(item.Value, _itemSlot);
        }

        protected virtual UniTask InitSlot(TItem item, TSlot slot) => slot.Init(item, Entity);
        
        private void Sell()
        {
            if (Entity.sellPrice <= 0)
                return;
        }
    }
}