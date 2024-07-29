using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
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

        public readonly ReactiveProperty<TItem> item = new();

        private UniTask _updateTask;
        
        public override UniTask Open(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }
            
            if (parameters[0] is not TItem data)
            {
                var exception = new InvalidCastException("첫 번째 인자를 아이템 데이터로 변환하지 못했습니다.");
                return UniTask.FromException(exception);
            }

            item.Value = data;
            
            return _updateTask;
        }

        private void OnDataInitializedInternal(TItem data)
        {
            if (data == null)
                return;

            if (!Storage.db.items.TryFind(data.ItemId, out var entity))
            {
                var exception = new Exception($"아이템을 찾을 수 없습니다. id={data.ItemId}");
                throw exception;
            }

            Entity = entity;
            _sell.gameObject.SetActive(Entity.sellPrice > 0);
            _updateTask = InitSlot(_itemSlot);
            OnDataInitialized(data);
        }

        protected virtual UniTask InitSlot(TSlot slot) => slot.Init(item.Value, Entity);

        protected abstract void OnDataInitialized(TItem data);
        
        protected override void OnAwake()
        {
            base.OnAwake();

            item.Subscribe(OnDataInitializedInternal)
                .AddTo(this);
            
            this.SubscribeButton(_sell, Sell);
        }
        
        private void Sell()
        {
            if (Entity.sellPrice <= 0)
                return;
            
            // TODO 판매 로직.
        }
    }
}