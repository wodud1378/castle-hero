using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;

namespace RGLabs.Common.UI
{
    public class UIInventorySlot : UIItemSlot
    {
        public async UniTask InitAsync(IItemEntity entity, CancellationToken ct)
        {
            await base.InitAsync(entity.Icon, string.Empty, ct);
        }
    }
}