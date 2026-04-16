using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Network.Shared;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Lobby.UI.Adapter
{
    public class UIItemList : UIListAdapter<UIItemSlot, IItem>
    {
        protected override UniTask SetItem(UIItemSlot slot, IItem data)
        {
            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(data.ItemId, out var entity))
                return UniTask.CompletedTask;

            return slot.Init(data, entity);
        }
    }
}