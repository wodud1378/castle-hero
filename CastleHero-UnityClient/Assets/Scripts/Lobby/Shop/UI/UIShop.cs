using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShop : UIMain
    {
        [SerializeField] private UIShopCategoryList _itemList;
        
        public void Init()
        {
            var categoryMap = new Dictionary<int, List<ShopItemEntity>>();
            Storage.db.shop.ForEach(x =>
            {
                if (!categoryMap.TryGetValue(x.category, out var list))
                {
                    list = new() { x };
                    categoryMap.Add(x.category, list);
                }
                else
                {
                    int index = list.FindIndex(exist => exist.Id == x.Id);
                    if(!index.IsValidIndex(list))
                        list.Add(x);
                }
            });

            _itemList
                .Init(categoryMap.Values)
                .Forget();
        }
    }
}