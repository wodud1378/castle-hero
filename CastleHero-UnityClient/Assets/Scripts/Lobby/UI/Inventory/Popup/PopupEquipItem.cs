using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Equipment.prefab")]

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

        private async void OpenElementalStoneList()
        {
            if (!Context.popups.TryGetPopupIfExist(out PopupInventory inventory))
                inventory = await Context.popups.OpenAsync<PopupInventory>();
            
            inventory.filter.Clear();
            inventory.filter.Add(PopupInventory.Category.Ingredient);
            inventory.customFilter.Value = (_, entity) => entity.optionConsume.type == ConsumeType.ElementalStone;
            inventory.refineParam.item = Item;
        }

        protected override UniTask InitSlot(EquipItem item, UIEquipmentSlot slot) => slot.Init(item);

        private async void OpenCharacterList()
        {
            var popup = await Context.popups.OpenAsync<PopupCharacterList>();
            popup.clickMethod = PopupCharacterList.ClickMethod.Equip;
            popup.equipmentId = Item.ItemId;
            
            Close();
        }

        private async void Release()
        {
            var unit = Storage.userRepository.characters.units.FirstOrDefault(x => x.id == Item.character);
            if (unit == null)
                return;

            var result = await NetworkService.Character.Release(unit.id, Item.Guid);
            if (!result.IsSuccess)
            {
                Context.popups.Open<PopupCommon>(result.error);
            }
        }

        protected override void OnDataChanged(EquipItem data)
        {
            base.OnDataChanged(data);

            bool onUse = data.character != 0;
            _equip.gameObject.SetActive(!onUse);
            _release.gameObject.SetActive(onUse);
            
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