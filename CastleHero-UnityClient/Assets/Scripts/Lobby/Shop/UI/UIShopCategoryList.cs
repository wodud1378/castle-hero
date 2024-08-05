using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopCategoryList : UIListAdapter<UIShopCategorySlot, IEnumerable<ShopItemEntity>>
    {
        protected override UniTask SetItem(UIShopCategorySlot slot, IEnumerable<ShopItemEntity> data) => slot.Init(data);
    }
}