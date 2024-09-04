using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Lobby.UI.Adapter;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/ReceiveResult.prefab")]
    public class PopupReceivedItems : PopupBase
    {
        [SerializeField] private UIItemList _itemList;
        
        public override UniTask Open(params object[] parameters)
        {
            var items = new List<IItem>();
            var currency = new CurrencyDto();
            foreach (var parameter in parameters)
            {
                switch (parameter)
                {
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
            items.AddRange(currencyItems.Where(x=> x.Quantity > 0));
            items.Sort((a, b)=> a.ItemId.CompareTo(b.ItemId));
            
            return _itemList.Init(items);
        }
    }
}