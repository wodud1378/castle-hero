using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;

namespace RGLabs.Lobby.UI.Inventory
{
    public class UIInventorySlot : UIItemSlot
    {
        public async UniTask InitAsync(IItemEntity entity)
        {
            await base.InitAsync(entity.Icon);
        }
    }
}