using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Model;
using RGLabs.Unit;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Equipment.prefab")]

    public class PopupEquipItem : PopupItemBase<UIEquipmentSlot, EquipItem, EquipmentEntity>
    {
        [SerializeField] private UIStatusText[] _stats;

        [SerializeField] private Button _refine;
        [SerializeField] private Button _equip;
        [SerializeField] private Button _release;

        protected override void OnAwake()
        {
            base.OnAwake();
            
            this.SubscribeButton(_refine, OpenElementalStoneList);
            this.SubscribeButton(_equip, OpenCharacterList);
            this.SubscribeButton(_release, Release);
        }

        protected override UniTask InitSlot(UIEquipmentSlot slot) => slot.Init(Item, Entity);

        private void OpenElementalStoneList()
        {
        }
        
        private async void OpenCharacterList()
        {
            var popup = await Context.popupManager.Open<PopupCharacterList>();
            popup.clickMethod = PopupCharacterList.ClickMethod.Equip;
            popup.equipmentId = Item.ItemId;
        }

        private void Release()
        {
        }

        protected override void OnDataInitialized()
        {
            foreach (var label in _stats)
            {
                var type = (int)label.type;
                int index = Array.FindIndex(Item.stats, x => x == type);
                if (index.IsValidIndex(Item.stats, Item.values))
                {
                    label.SetText(Item.values[index]);
                    label.gameObject.SetActive(true);
                }
                else 
                    label.gameObject.SetActive(false);
            }
        }
    }
}