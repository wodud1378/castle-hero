using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.Model;

namespace RGLabs.Common.UI
{
    public class UICharacterList : UIListAdapter<UICharacterSlot, UnitInfo>
    {
        protected override async UniTask SetItem(UICharacterSlot item, UnitInfo data, CancellationToken ct)
        {
            if (!Storage.db.units.TryFind(data.id, out var entity))
                return;
            
            await item.InitAsync(data, entity, ct);
        }
    }
}