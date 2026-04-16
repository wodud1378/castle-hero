using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data.Model;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.Shop.UI
{
    public class UIShopCategorySlot : UISlot
    {
        [FormerlySerializedAs("_itemList")]
        [SerializeField] private UIShopItemList itemList;

        public UniTask Init(IEnumerable<ShopItemEntity> entities)
        {
            var array = entities.ToArray();

            return UniTask.WhenAll(
                    base.Init(string.Empty, array[0].CategoryText()),
                    itemList.Init(array));
        }
    }
}
