using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Network.Shared;

namespace RGLabs.Lobby.UI.Inventory
{
    public class UIInventoryItemList : UIListAdapter<UIItemSlot, IItem>
    {
        protected override UniTask SetItem(UIItemSlot slot, IItem data)
        {
            if (!Storage.db.items.TryFind(data.ItemId, out var entity))
                return UniTask.CompletedTask;

            return slot.Init(data, entity);
        }
    }
}