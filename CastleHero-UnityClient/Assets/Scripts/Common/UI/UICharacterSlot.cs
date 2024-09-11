using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;

namespace RGLabs.Common.UI
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
            if (!Storage.db.units.TryFind(info.id, out var entity))
                return UniTask.CompletedTask;

            return Init(info, entity);
        }
    }
}