using CastleHero.View.Common.UI;
using CastleHero.Network.Shared;

namespace CastleHero.View.Lobby.UI.Adapter
{
    public class UISummonedList : UIListAdapter<UISummonSlot, ISummoned>
    {
        protected override void SetItem(UISummonSlot slot, ISummoned data) => slot.Init(data);
    }
}