using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopCategorySlot : UISlot
    {
        [SerializeField] private UIShopItemList _itemList;

        public UniTask Init(IEnumerable<ShopItemEntity> entities) => _itemList.Init(entities);
    }
}