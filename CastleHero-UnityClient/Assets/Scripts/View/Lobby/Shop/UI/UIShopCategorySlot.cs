using System.Collections.Generic;
using System.Linq;
using CastleHero.Common.Localize;
using CastleHero.Common.Pattern;
using CastleHero.View.Common.UI;
using CastleHero.Data.Model;
using CastleHero.Data.Shared;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.Shop.UI
{
    public class UIShopCategorySlot : UISlot
    {
        [FormerlySerializedAs("_itemList")]
        [SerializeField] private UIShopItemList itemList;

        private LocalizeText _localize;

        private void Awake()
        {
            _localize = ServiceLocator.Instance.Get<LocalizeText>();
        }

        public void Init(IEnumerable<ShopItemEntity> entities)
        {
            var array = entities.ToArray();

            base.Init(string.Empty, array[0].CategoryText(_localize));
            itemList.Init(array);
        }
    }
}
