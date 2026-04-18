using CastleHero.Data;
using CastleHero.Network.Shared;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Common.UI
{
    public class UICharacterList : UIListAdapter<UICharacterSlot, UnitInfo>
    {
        private IDBProvider _db;

        private void Awake()
        {
            _db = ServiceLocator.Instance.Get<IDBProvider>();
        }

        protected override void SetItem(UICharacterSlot slot, UnitInfo data)
        {
            if (!_db.Units.TryFind(data.id, out var entity))
                return;

            slot.Init(data, entity);
        }
    }
}