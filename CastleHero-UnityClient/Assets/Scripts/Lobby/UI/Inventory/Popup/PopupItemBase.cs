using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    public abstract class PopupItemBase<TSlot, TItem, TEntity> : PopupBase
        where TSlot : UIItemSlot
        where TItem : IItem
        where TEntity : IItemEntity
    {
        [SerializeField] private TSlot _itemSlot;
        [SerializeField] private Button _sell;

        public TItem Item { get; private set; }
        public TEntity Entity { get; private set; }

        public override UniTask Open(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 1)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }
            
            if (parameters[0] is not TItem item)
            {
                var exception = new InvalidCastException("첫 번째 인자를 아이템 데이터로 변환하지 못했습니다.");
                return UniTask.FromException(exception);
            }

            if (!Storage.db.itemDBAccessor.TryLoad(item.Id, out var entity))
            {
                var exception = new Exception($"아이템을 찾을 수 없습니다. id={item.Id}");
                return UniTask.FromException(exception);
            }

            Item = item;
            Entity = (TEntity)entity;
            
            _sell.gameObject.SetActive(Entity.SellPrice > 0);
            
            OnDataInitialized();
            
            return InitSlot(_itemSlot);
        }

        protected virtual UniTask InitSlot(TSlot slot) => slot.Init(Item, Entity);

        protected abstract void OnDataInitialized();
        
        protected override void InitSubscriptions()
        {
            base.InitSubscriptions();
            
            this.SubscribeButton(_sell, Sell);
        }
        
        private void Sell()
        {
            if (Entity.SellPrice <= 0)
                return;
            
            // TODO 판매 로직.
        }
    }
}