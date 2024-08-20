using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopCategoryList : UIListAdapter<UIShopCategorySlot, IEnumerable<ShopItemEntity>>
    {
        [SerializeField] private AssetReference[] _categorySlots;

        protected override async UniTask<UIShopCategorySlot> ProvideSlot(IEnumerable<ShopItemEntity> data)
        {
            int index = (int)data.First().category;
            var asset = _categorySlots[index];

            return await asset.Instantiate<UIShopCategorySlot>(itemRoot);
        }

        protected override UniTask SetItem(UIShopCategorySlot slot, IEnumerable<ShopItemEntity> data) => slot.Init(data);
    }
}