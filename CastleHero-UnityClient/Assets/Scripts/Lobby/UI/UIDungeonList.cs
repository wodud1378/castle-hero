using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using RGLabs.Network.Service;

namespace RGLabs.Lobby.UI
{
    public class UIDungeonList : UIListAdapter<UIDungeonSlot, (DungeonType type, DungeonDetailType detailType)>
    {
        protected override UniTask SetItem(UIDungeonSlot slot, (DungeonType type, DungeonDetailType detailType) data) 
            => slot.Init(data.type, data.detailType);
    }
}