using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data.Model;
using CastleHero.Network.Service;

namespace CastleHero.View.Lobby.UI
{
    public class UIDungeonList : UIListAdapter<UIDungeonSlot, DungeonEntity>
    {
        protected override UniTask SetItem(UIDungeonSlot slot, DungeonEntity data) => slot.Init(data);
    }
}