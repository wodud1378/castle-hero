using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;

namespace RGLabs.Lobby.UI
{
    public class UIDungeonList : UIListAdapter<UIDungeonSlot, DungeonEntity>
    {
        protected override UniTask SetItem(UIDungeonSlot slot, DungeonEntity data)
        {
            throw new System.NotImplementedException();
        }
    }
}