using System;
using System.Linq;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.View.Lobby.UI.Popup;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using CastleHero.Common.Pattern;

using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
using Cysharp.Threading.Tasks;
namespace CastleHero.View.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Use.prefab")]
    public class PopupUseItem : PopupItemBase<UIItemSlot, IItem>
    {
        [FormerlySerializedAs("_countRoot")]
        [SerializeField] private GameObject countRoot;
        [FormerlySerializedAs("_increase")]
        [SerializeField] private Button increase;
        [FormerlySerializedAs("_decrease")]
        [SerializeField] private Button decrease;
        [FormerlySerializedAs("_slider")]
        [SerializeField] private Slider slider;

        [FormerlySerializedAs("_use")]
        [SerializeField] private Button use;
        [FormerlySerializedAs("_description")]
        [SerializeField] private TMP_Text description;
        [FormerlySerializedAs("_useCount")]
        [SerializeField] private TMP_Text useCount;
        [FormerlySerializedAs("_effect")]
        [SerializeField] private TMP_Text effect;

        protected override int SellCount => (int)slider.value;

        private IPopupManager _popups;
        private StateManager<LobbyState> _lobbyState;

        protected override void OnAwake()
        {
            base.OnAwake();

            _popups = ServiceLocator.Get<IPopupManager>();
            _lobbyState = ServiceLocator.Get<StateManager<LobbyState>>();

            this.SubscribeButton(increase, () => slider.value = Mathf.Min(slider.value + 1, slider.maxValue));
            this.SubscribeButton(decrease, () => slider.value = Mathf.Min(slider.value - 1, slider.minValue));
            this.SubscribeButton(use, OnUse);

            slider.onValueChanged
                .AsObservable()
                .Subscribe(UpdateWithQuantity)
                .AddTo(this);

            slider.value = 0f;
        }

        protected override void OnDataChanged(IItem data)
        {
            base.OnDataChanged(data);

            description.text = Entity.desc[0];

            SetActiveSlider();

            if (slider.gameObject.activeSelf)
            {
                var last = slider.value;
                slider.value = Mathf.Clamp(last, 0, slider.maxValue);
            }
        }

        private void UpdateWithQuantity(float value)
        {
            int toInt = (int)value;
            useCount.text = (toInt).ToString();
            UpdateEffectText(toInt);
        }

        private void UpdateEffectText(int count)
        {
            if (!string.IsNullOrEmpty(Entity.desc[1]))
            {
                effect.text = string.Format(Entity.desc[1], int.Parse(Entity.options[1]) * count)
                    .WithPositiveColor();

                effect.gameObject.SetActive(true);
            }
            else
                effect.gameObject.SetActive(false);
        }

        private void SetActiveSlider()
        {
            bool hasPrice = Entity.sellPrice > 0;
            bool usable = false;
            var type = Entity.type;
            int maxCount = Item.Quantity;
            switch (type)
            {
                case ItemType.Consumable:
                    usable = Entity.optionConsume.type == ConsumeType.Stamina;
                    break;
                case ItemType.Ingredient:
                    var option = Entity.optionIngredient;
                    bool isPiece = option.type is IngredientType.ElementalPiece or IngredientType.EquipmentPiece;
                    usable = isPiece && Item.Quantity >= option.forCombine;
                    maxCount = isPiece ? Item.Quantity / option.forCombine : maxCount;
                    break;
                case ItemType.Chest:
                    usable = true;
                    break;
            }

            bool isActive = hasPrice || usable;
            useCount.gameObject.SetActive(isActive);
            countRoot.gameObject.SetActive(isActive);
            slider.minValue = 1;
            slider.maxValue = maxCount;
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
                case ConsumeType.Exp:
                    MoveToLvUp();
                    break;
                case ConsumeType.Stamina:
                    AddStamina();
                    break;
                case ConsumeType.SummonTicket:
                    MoveToDrawCharacter(Item.ItemId);
                    break;
                case ConsumeType.ElementalStone:
                    Refine();
                    break;
                case ConsumeType.PlayTicket:
                    MoveToStage();
                    break;
            }
        }

        private void UseIngredient(IngredientOption option)
        {
            switch (option.type)
            {
                case IngredientType.Soul:
                    int unitId = (Entity.Id % 1000) + 10000;
                    var unit = ServiceLocator.Get<IUserRepository>().Characters.Units.FirstOrDefault(x => x.id == unitId);

                    if (unit != null)
                        _popups.Open<PopupRateUp>(unit);
                    break;
                case IngredientType.ElementalPiece:
                case IngredientType.EquipmentPiece:
                    Combine();
                    break;
            }
        }

        private async UniTask MoveToLvUp()
        {
            var characters = await _popups.OpenAsync<PopupCharacterList>();
            characters.BeginSelect(true);

            var selected = await characters.SelectTask;
            if (selected == null)
                return;

            _popups.Open<PopupLevelUp>(selected, Entity.Id);
        }

        private void MoveToStage()
        {
            _lobbyState.CurrentState = LobbyState.Prepare;

            _popups.CloseAll();
        }

        private async UniTask Combine()
        {
            var result = await ServiceLocator.Get<INetworkServiceProvider>().Inventory.Combine(Item.ItemId, (int)slider.value);
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return;
            }

            _popups.Open<PopupReceivedItems>(result.data);
        }

        private async UniTask OpenBox()
        {
            var result = await ServiceLocator.Get<INetworkServiceProvider>().Inventory.OpenChest(Item.ItemId, (int)slider.value);
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return;
            }

            var data = result.data;
            var currency = data.currency;
            var items = data.items;

            _popups.Open<PopupReceivedItems>(currency, items);
        }

        private async UniTask AddStamina()
        {
            var result = await ServiceLocator.Get<INetworkServiceProvider>().Inventory.AddStamina(Item.ItemId, (int)slider.value);
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
            }
        }

        private void MoveToDrawCharacter(int ticketId)
        {
            if (!ServiceLocator.Get<IDBProvider>().Summons.TryFind(x => x.costItems.Contains(ticketId), out var entity))
                return;

            _popups.Open<PopupSummon>(entity);
            Close();
        }

        private async UniTask Refine()
        {
            var items = ServiceLocator.Get<IUserRepository>().Inventory.Items
                .OfType<EquipItem>();

            var selection = await _popups.OpenAsync<PopupSelectItem>(items);

            bool closed = false;
            EquipItem equipItem = null;
            while (!closed && equipItem == null)
            {
                selection.BeginSelect(false);

                var selected = await selection.SelectTask;
                if (selected == null)
                    closed = true;
                else
                {
                    equipItem = selected as EquipItem;
                }
            }

            if (closed)
                return;

            selection.Close();

            _popups.Open<PopupRefine>(equipItem, Entity);
        }
    }
}
