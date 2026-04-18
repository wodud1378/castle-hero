using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Network.Shared;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Lobby.UI.Adapter
{
    public class UIItemList : UIListAdapter<UIItemSlot, IItem>
    {
        private IDBProvider _db;

        private void Awake()
        {
            var sl = ServiceLocator.Instance;
            _db = sl.Get<IDBProvider>();
        }

        protected override void SetItem(UIItemSlot slot, IItem data)
        {
            if (!_db.Items.TryFind(data.ItemId, out var entity))
                return;

            slot.Init(data, entity);
        }
    }
}