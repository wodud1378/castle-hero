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
        [SerializeField] private Slider _slider;
        [SerializeField] private Button _use;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private TMP_Text _useCount;
        [SerializeField] private TMP_Text _effect;
        
        protected override void OnAwake()
        {
            base.OnAwake();

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
            bool isActive = false;
            var type = Entity.type;
            switch (type)
            {
                case ItemType.Consumable:
                    var option = (ConsumeType)int.Parse(Entity.options[0]);
                    isActive = option is ConsumeType.Stamina;
                    break;
                case ItemType.Ingredient:
                case ItemType.Chest:
                    isActive = true;
                    break;
            }

            _useCount.gameObject.SetActive(isActive);
            _slider.gameObject.SetActive(isActive);
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
                    Combine();
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
                    MoveToRefine();
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

        private async void MoveToRefine()
        {
            if (!Context.popups.TryGetPopupIfExist(out PopupInventory popup))
                popup = await Context.popups.OpenAsync<PopupInventory>();
            
            popup.mode.Value = PopupInventory.Mode.Refine;
            popup.refineParam.entity = Entity;
            Close();
        }
    }
}