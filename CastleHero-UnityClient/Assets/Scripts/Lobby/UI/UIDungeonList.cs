using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using RGLabs.Network.Service;

namespace RGLabs.Lobby.UI
{
    public class UIDungeonList : UIListAdapter<UIDungeonSlot, DungeonEntity>
    {
        protected override UniTask SetItem(UIDungeonSlot slot, DungeonEntity data)
        {
            var openDays = data.OpenDaysOfWeek();
            var dow = NetworkService.CurrentTime().DayOfWeek;

            slot.state.Value = openDays.Contains(dow)
                ? UIState.State.Default
                : UIState.State.Dim;
            
            return slot.Init(data);
        }
    }
}