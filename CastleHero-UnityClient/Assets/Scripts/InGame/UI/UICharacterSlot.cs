using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.DB;
using RGLabs.Network.Model;

namespace RGLabs.InGame.UI
{
    public class UICharacterSlot : UIItemSlot
    {
        public UnitInfo Info { get; private set; }
 
        public async UniTask InitAsync(UnitInfo info, UnitDB db)
        {
            Info = info;

            if (!db.TryFind(info.id, out var entity))
                return;
            
            await base.InitAsync(entity.icon);
        }
    }
}