using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Lobby.Shop.UI
{
    public class UIShopCategorySlot : UISlot
    {
        [SerializeField] private UIShopItemList _itemList;

        public UniTask Init(IEnumerable<ShopItemEntity> entities)
        {
            var array = entities.ToArray();
            
            return UniTask.WhenAll(
                    base.Init(string.Empty, array[0].CategoryText()),
                    _itemList.Init(array));
        }
    }
}