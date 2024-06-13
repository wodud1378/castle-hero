using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Inventory.Popup;
using RGLabs.Network.Model;
using RGLabs.Utility;

namespace RGLabs.Lobby.UI.Inventory
{
    public class UIInventoryItemList : UIListAdapter<UIItemSlot, IItem>
    {
        protected override UniTask SetItem(UIItemSlot slot, IItem data)
        {
            var accessor = Storage.db.itemDBAccessor;
            if (!accessor.TryLoad(data.Id, out var entity))
                return UniTask.CompletedTask;

            return slot.Init(data, entity);
        }

        private void OnClickItem(UIItemSlot slot)
        {
            var item = slot.Item;
            var type = item.Id.ItemType();
            switch (type)
            {
                case ItemTypeCode.Equipment:
                    Context.popupManager.Open<PopupEquipItem>(item).Forget();
                    break;
                case ItemTypeCode.Consumable:
                    break;
                case ItemTypeCode.Ingredient:
                    break;
            }
        }
    }
}