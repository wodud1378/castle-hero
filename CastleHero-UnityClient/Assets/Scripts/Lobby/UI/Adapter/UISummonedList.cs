using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Network.Shared;

namespace RGLabs.Lobby.UI.Adapter
{
    public class UISummonedList : UIListAdapter<UISummonSlot, ISummoned>
    {
        protected override UniTask SetItem(UISummonSlot slot, ISummoned data) => slot.Init(data);
    }
}