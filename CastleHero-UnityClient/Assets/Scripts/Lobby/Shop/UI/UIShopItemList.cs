using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopItemList : UIListAdapter<UIShopItemSlot, ShopItemEntity>
    {
        protected override UniTask SetItem(UIShopItemSlot slot, ShopItemEntity data) => slot.Init(data);
    }
}