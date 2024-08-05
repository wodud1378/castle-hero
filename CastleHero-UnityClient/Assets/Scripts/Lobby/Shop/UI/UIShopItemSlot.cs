using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopItemSlot : UISlot
    {
        public int Id { get; private set; }

        public UniTask Init(ShopItemEntity entity)
        {
            Id = entity.Id;

            return base.Init(entity.image);
        }
    }
}