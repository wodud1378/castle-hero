using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Shared;
using RGLabs.Unit;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Equipment.prefab")]

    public class PopupEquipItem : PopupItemBase<UIEquipmentSlot, EquipItem>
    {
        [SerializeField] private UIStatusText[] _mainStat; 
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

        private void OpenElementalStoneList()
        {
        }
        
        private async void OpenCharacterList()
        {
            var popup = await Context.popups.OpenAsync<PopupCharacterList>();
            popup.clickMethod = PopupCharacterList.ClickMethod.Equip;
            popup.equipmentId = item.Value.ItemId;
        }

        private void Release()
        {
        }

        protected override void OnDataInitialized(EquipItem data)
        {
            foreach (var label in _mainStat)
            {
                var main = data.main;
                bool matches = (int)label.type == main.type;
                if (matches)
                {
                    label.SetText(main.value);
                    label.gameObject.SetActive(true);
                }
                else
                {
                    label.gameObject.SetActive(false);
                }
            }
            
            foreach (var label in _stats)
            {
                var type = (int)label.type;
                
                var stat = data.sub.Find(x => x.type == type);
                if(stat != null)
                {
                    label.SetText(stat.value);
                    label.gameObject.SetActive(true);
                }
                else 
                    label.gameObject.SetActive(false);
            }
        }
    }
}