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
            if (!accessor.TryLoad(data.ItemId, out var entity))
                return UniTask.CompletedTask;

            return slot.Init(data, entity);
        }
    }
}