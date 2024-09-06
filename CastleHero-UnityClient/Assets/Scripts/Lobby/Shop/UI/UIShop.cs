using System.Collections.Generic;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Service;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShop : UIMain
    {
        [SerializeField] private UIShopCategoryList _itemList;
        [SerializeField] private RectTransform _withdrawalRoot;

        protected override void OnBack()
        {
            Context.Transition.CurrentState = State.Lobby;
        }

        public async void Init()
        {
            await NetworkService.Shop.RefreshProducts();
            
            var categoryMap = new Dictionary<ShopCategory, List<ShopItemEntity>>();
            var currentTime = NetworkService.CurrentTimeByLocal();
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

            _withdrawalRoot.gameObject.SetActive(false);

            foreach (var kvp in categoryMap)
            {
                kvp.Value.Sort((x, y) => x.order.CompareTo(y.order));
            }
            
            await _itemList.Init(categoryMap.Values);
            
            _withdrawalRoot.gameObject.SetActive(true);
            _withdrawalRoot.transform.SetAsLastSibling();
        }
    }
}