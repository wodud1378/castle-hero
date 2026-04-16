using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Network.Shared;

namespace CastleHero.View.Lobby.UI.Adapter
{
    public class UISummonedList : UIListAdapter<UISummonSlot, ISummoned>
    {
        protected override UniTask SetItem(UISummonSlot slot, ISummoned data) => slot.Init(data);
    }
}