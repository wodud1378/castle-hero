using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using UnityEngine.AddressableAssets;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopItemList : UIListAdapter<UIShopItemSlot, ShopItemEntity>
    {
        protected override async UniTask<UIShopItemSlot> ProvideSlot(ShopItemEntity data)
        {
            var obj = await Addressables.InstantiateAsync(data.prefab, itemRoot);

            return obj.GetComponent<UIShopItemSlot>();
        }

        protected override UniTask SetItem(UIShopItemSlot slot, ShopItemEntity data) => slot.Init(data);
    }
}