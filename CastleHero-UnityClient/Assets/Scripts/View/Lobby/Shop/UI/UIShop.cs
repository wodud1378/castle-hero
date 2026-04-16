using System.Collections.Generic;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using CastleHero.Common.Pattern;

using CastleHero.Data.DB;
using Cysharp.Threading.Tasks;
namespace CastleHero.View.Lobby.Shop.UI
{
    public class UIShop : UIMain
    {
        [FormerlySerializedAs("_itemList")]
        [SerializeField] private UIShopCategoryList itemList;
        [FormerlySerializedAs("_withdrawalRoot")]
        [SerializeField] private RectTransform withdrawalRoot;

        protected override void OnBack()
        {
            ServiceLocator.Get<StateManager<LobbyState>>().CurrentState = LobbyState.Main;
        }

        public async UniTask Init()
        {
            await ServiceLocator.Get<INetworkServiceProvider>().Shop.RefreshProducts();

            var categoryMap = new Dictionary<ShopCategory, List<ShopItemEntity>>();
            var currentTime = ServerTime.Now;
            ServiceLocator.Get<IDBProvider>().Shop.ForEach(x =>
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

            withdrawalRoot.gameObject.SetActive(false);

            foreach (var kvp in categoryMap)
            {
                kvp.Value.Sort((x, y) => x.order.CompareTo(y.order));
            }

            await itemList.Init(categoryMap.Values);

            withdrawalRoot.gameObject.SetActive(true);
            withdrawalRoot.transform.SetAsLastSibling();
        }
    }
}
