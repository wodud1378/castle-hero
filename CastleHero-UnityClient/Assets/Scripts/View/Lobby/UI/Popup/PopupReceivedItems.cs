using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI.Popup;
using CastleHero.View.Lobby.UI.Adapter;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Receive.prefab")]
    public class PopupReceivedItems : PopupBase
    {
        [FormerlySerializedAs("_itemList")]
        [SerializeField] private UIItemList itemList;

        public override UniTask Open(params object[] parameters)
        {
            var items = new List<IItem>();
            var currency = new CurrencyDto();
            foreach (var parameter in parameters)
            {
                switch (parameter)
                {
                    case Pack pack:
                        items.AddRange(pack.items);
                        currency += pack.currency;
                        break;
                    case IItem item:
                        items.Add(item);
                        break;
                    case IEnumerable<IItem> list:
                        items.AddRange(list);
                        break;
                    case CurrencyDto c:
                        currency += c;
                        break;
                }
            }

            var currencyItems = currency.ToItems();
            items.AddRange(currencyItems.Where(x => x.Quantity > 0));
            items.Sort((a, b) => a.ItemId.CompareTo(b.ItemId));

            return itemList.Init(items);
        }
    }
}
