using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopCategorySlot : UISlot
    {
        [SerializeField] private UIShopItemList _itemList;
        [SerializeField] private AssetReference _slotBySingle;
        [SerializeField] private AssetReference _slotByDouble;
        [SerializeField] private AssetReference _slotByMultiple;

        public UniTask Init(IEnumerable<ShopItemEntity> entities)
        {
            _itemList.provideSlot = ProvideSlot;

            var array = entities.ToArray();
            
            return UniTask.WhenAll(
                    base.Init(string.Empty, array[0].CategoryText()),
                    _itemList.Init(array));
        }

        private AssetReference ProvideSlot(ShopItemEntity entity)
        {
            var asset = entity.category switch
            {
                ShopCategory.NoAds or
                    ShopCategory.Package or
                    ShopCategory.BattlePass => _slotBySingle,

                ShopCategory.MonthlyFee => _slotByDouble,
                _ => _slotByMultiple
            };

            return asset;
        }
    }
}