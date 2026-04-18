using CastleHero.View.Common.UI;
using CastleHero.Data.Model;

namespace CastleHero.View.Lobby.UI
{
    public class UIDungeonList : UIListAdapter<UIDungeonSlot, DungeonEntity>
    {
        protected override void SetItem(UIDungeonSlot slot, DungeonEntity data) => slot.Init(data);
    }
}