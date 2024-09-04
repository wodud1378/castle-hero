using System;
using System.Linq;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Use.prefab")]
    public class PopupUseItem : PopupItemBase<UIItemSlot, IItem>
    {
        [SerializeField] private GameObject _countRoot;
        [SerializeField] private Button _increase;
        [SerializeField] private Button _decrease;
        [SerializeField] private Slider _slider;
        
        [SerializeField] private Button _use;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private TMP_Text _useCount;
        [SerializeField] private TMP_Text _effect;

        protected override int SellCount => (int)_slider.value;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_increase, ()=> _slider.value = Mathf.Min(_slider.value + 1, _slider.maxValue) );
            this.SubscribeButton(_decrease, ()=> _slider.value = Mathf.Min(_slider.value - 1, _slider.minValue) );
            this.SubscribeButton(_use, OnUse);

            _slider.onValueChanged
                .AsObservable()
                .Subscribe(UpdateWithQuantity)
                .AddTo(this);
            
            _slider.value = 0f;
        }

        protected override void OnDataChanged(IItem data)
        {
            base.OnDataChanged(data);
            
            _description.text = Entity.desc[0];

            SetActiveSlider();

            if (_slider.gameObject.activeSelf)
            {
                var last = _slider.value;
                _slider.value = Mathf.Clamp(last, 0, _slider.maxValue);
            }
        }

        private void UpdateWithQuantity(float value)
        {
            int toInt = (int)value;
            _useCount.text = (toInt).ToString();
            UpdateEffectText(toInt);
        }

        private void UpdateEffectText(int count)
        {
            if (!string.IsNullOrEmpty(Entity.desc[1]))
            {
                _effect.text = string.Format(Entity.desc[1], int.Parse(Entity.options[1]) * count)
                    .WithPositiveColor();

                _effect.gameObject.SetActive(true);
            }
            else
                _effect.gameObject.SetActive(false);
        }

        private void SetActiveSlider()
        {
            bool hasPrice = Entity.sellPrice > 0;
            bool hasUseOption = false;
            var type = Entity.type;
            switch (type)
            {
                case ItemType.Consumable:
                    hasUseOption = Entity.optionConsume.type == ConsumeType.Stamina;
                    break;
                case ItemType.Ingredient:
                    hasUseOption =
                        Entity.optionIngredient.type is IngredientType.ElementalPiece or IngredientType.EquipmentPiece; 
                    break;
                case ItemType.Chest:
                    hasUseOption = true;
                    break;
            }
            
            bool isActive = hasPrice || hasUseOption;
            _useCount.gameObject.SetActive(isActive);
            _countRoot.gameObject.SetActive(isActive);
            _slider.maxValue = Item.Quantity;
        }

        private void OnUse()
        {
            switch (Entity.type)
            {
                case ItemType.Consumable:
                    Consume(Entity.optionConsume);
                    break;
                case ItemType.Ingredient:
                    UseIngredient(Entity.optionIngredient);
                    break;
                case ItemType.Chest:
                    OpenBox();
                    break;
            }
        }

        private void Consume(ConsumableOption option)
        {
            switch (option.type)
            {
                case ConsumeType.Stamina:
                    AddStamina();
                    break;
                case ConsumeType.SummonTicket:
                    MoveToDrawCharacter(Item.ItemId);
                    break;
                case ConsumeType.ElementalStone:
                    Refine();
                    break;
            }
        }

        private void UseIngredient(IngredientOption option)
        {
            switch (option.type)
            {
                case IngredientType.Soul:
                    int unitId = (Entity.Id % 1000) + 10000;
                    var unit = Storage.userRepository.characters.units.FirstOrDefault(x => x.id == unitId);
                    
                    if(unit != null)
                        Context.popups.Open<PopupRateUp>(unit);
                    break;
                case IngredientType.ElementalPiece:
                case IngredientType.EquipmentPiece:
                    Combine();
                    break;
            }
        }

        private async void Combine()
        {
            var result = await NetworkService.Inventory.Combine(Item.ItemId, (int)_slider.value);
            if (!result.IsSuccess)
            {
                Context.popups.Open<PopupCommon>(result.error);
                return;
            }
            
            Context.popups.Open<PopupReceivedItems>(result.data);
        }

        private async void OpenBox()
        {
            var result = await NetworkService.Inventory.OpenChest(Item.ItemId, (int)_slider.value);
            if (!result.IsSuccess)
            {
                Context.popups.Open<PopupCommon>(result.error);
                return;
            }

            var data = result.data;
            var currency = data.currency;
            var items = data.items;
            
            Context.popups.Open<PopupReceivedItems>(currency, items);
        }

        private async void AddStamina()
        {
            var result = await NetworkService.Inventory.AddStamina(Item.ItemId, (int)_slider.value);
            if (!result.IsSuccess)
            {
                Context.popups.Open<PopupCommon>(result.error);
            }
        }

        private void MoveToDrawCharacter(int ticketId)
        {
            if (!Storage.db.summons.TryFind(x => x.costItems.Contains(ticketId), out var entity))
                return;

            Context.popups.Open<PopupSummon>(entity);
            Close();
        }

        private async void Refine()
        {
            if (!Context.popups.TryGetPopupIfExist(out PopupInventory inventory))
                inventory = await Context.popups.OpenAsync<PopupInventory>();
            else
                Context.popups.ReplaceToTop(inventory);

            inventory.customFilter.Value = (x, _) =>
                x is EquipItem e &&
                (EquipmentSlot)e.slot is EquipmentSlot.Weapon or EquipmentSlot.Armor;

            bool closed = false;
            EquipItem equipItem = null;
            while (!closed && equipItem ==null)
            {
                inventory.BeginSelect(false);

                var selected = await inventory.SelectTask;
                if (selected == null)
                    closed = true;
                else
                {
                    equipItem = selected is EquipItem e ? e : null;
                }
            }
            
            if (closed)
                return;
            
            Context.popups.Open<PopupRefine>(equipItem, Entity);
        }
    }
}