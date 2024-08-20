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
            
            return slot.Init(data, openDays.Contains(dow)
                ? UIState.State.Default
                : UIState.State.Dim);
        }
    }
}