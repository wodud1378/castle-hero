using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Network.Shared;

namespace RGLabs.Lobby.UI.Adapter
{
    public class UIItemList : UIListAdapter<UIItemSlot, IItem>
    {
        protected override UniTask SetItem(UIItemSlot slot, IItem data)
        {
            if (!Storage.db.items.TryFind(data.ItemId, out var entity))
                return UniTask.CompletedTask;

            return slot.Init(data, entity);
        }
    }
}