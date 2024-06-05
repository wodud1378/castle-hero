using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Network.Model;

namespace RGLabs.Common.UI
{
    public class UICharacterSlot : UIItemSlot
    {
        public UnitInfo Info { get; private set; }
 
        public async UniTask InitAsync(UnitInfo info, UnitEntity entity, CancellationToken ct)
        {
            Info = info;
            
            await base.InitAsync(entity.icon, string.Empty, ct);
        }
    }
}