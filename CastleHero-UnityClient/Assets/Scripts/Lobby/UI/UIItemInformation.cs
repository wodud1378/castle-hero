using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public abstract class UIItemInformation<TItem, TEntity> : UIItemSlot
        where TItem : IItem
        where TEntity : IItemEntity
    {
        [SerializeField] private Button _sellButton;
        [SerializeField] private Button _useButton;

        public async UniTask InitAsync(TItem item)
        {
            var entity = Convert(item);

            await base.InitAsync(entity.Icon, entity.Desc);
            
            _sellButton.gameObject.SetActive(entity.SellPrice > 0);
            Construct(item, entity);
        }

        private TEntity Convert(TItem item)
        {
            var accessor = Storage.db.itemDBAccessor;
            if (!accessor.TryLoad(item.Id, out var entity))
                return default;

            return (TEntity)entity;
        }

        protected abstract void Construct(TItem item, TEntity entity);
    }
}