using Cysharp.Threading.Tasks;
using CastleHero.Data;
using CastleHero.Network.Shared;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Common.UI
{
    public class UICharacterList : UIListAdapter<UICharacterSlot, UnitInfo>
    {
        protected override UniTask SetItem(UICharacterSlot slot, UnitInfo data)
        {
            if (!ServiceLocator.Get<IDBProvider>().Units.TryFind(data.id, out var entity))
                return UniTask.CompletedTask;
            
            return slot.Init(data, entity);
        }
    }
}