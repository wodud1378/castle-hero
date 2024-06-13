using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Network.Model;

namespace RGLabs.Common.UI
{
    public class UICharacterSlot : UISlot
    {
        public UnitInfo Info { get; private set; }
 
        public UniTask InitAsync(UnitInfo info, UnitEntity entity)
        {
            Info = info;
            
            return Init(entity.icon);
        }
    }
}