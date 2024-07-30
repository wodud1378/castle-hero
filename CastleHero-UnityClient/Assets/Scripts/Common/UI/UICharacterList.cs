using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.Shared;

namespace RGLabs.Common.UI
{
    public class UICharacterList : UIListAdapter<UICharacterSlot, UnitInfo>
    {
        protected override UniTask SetItem(UICharacterSlot slot, UnitInfo data)
        {
            if (!Storage.db.units.TryFind(data.id, out var entity))
                return UniTask.CompletedTask;
            
            return slot.Init(data, entity);
        }
    }
}