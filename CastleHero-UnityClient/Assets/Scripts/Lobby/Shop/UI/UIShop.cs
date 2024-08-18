using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Service;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShop : UIMain
    {
        [SerializeField] private UIShopCategoryList _itemList;
        [SerializeField] private AssetReference _slotBySingle;
        [SerializeField] private AssetReference _slotByDouble;
        [SerializeField] private AssetReference _slotByMultiple;

        public void Init()
        {
            _itemList.provideSlot = ProvideSlot;

            var categoryMap = new Dictionary<ShopCategory, List<ShopItemEntity>>();
            var currentTime = NetworkService.CurrentTime();
            Storage.db.shop.ForEach(x =>
            {
                if (x.category == ShopCategory.Limited)
                    return;

                // 오픈되지 않은 상품.
                if (x.startDate > currentTime)
                    return;

                // 이미 종료된 상품.
                if (x.endDate != default && currentTime >= x.endDate)
                    return;

                if (!categoryMap.TryGetValue(x.category, out var list))
                {
                    list = new();
                    categoryMap.Add(x.category, list);
                }

                int index = list.FindIndex(exist => exist.Id == x.Id);
                if (!index.IsValidIndex(list))
                    list.Add(x);
            });

            _itemList
                .Init(categoryMap.Values)
                .Forget();
        }

        private AssetReference ProvideSlot(IEnumerable<ShopItemEntity> data)
        {
            var asset = data.First().category switch
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