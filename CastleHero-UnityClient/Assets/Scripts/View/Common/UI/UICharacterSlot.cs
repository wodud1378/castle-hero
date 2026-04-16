using Cysharp.Threading.Tasks;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Common.UI
{
    public class UICharacterSlot : UISlot
    {
        public UnitInfo Info { get; private set; }
 
        public UniTask Init(UnitInfo info, UnitEntity entity)
        {
            Info = info;
            
            return Init(entity.icon, $"Lv.{info.lv}");
        }
        
        public UniTask Init(UnitInfo info)
        {
            if (!ServiceLocator.Get<IDBProvider>().Units.TryFind(info.id, out var entity))
                return UniTask.CompletedTask;

            return Init(info, entity);
        }
    }
}