using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_LevelUp.prefab")]
    public class PopupLevelUp : PopupGrowth
    {
        private const int ExpSmallId = 52001;
        private const int ExpMediumId = 52002;
        private const int ExpLargeId = 52003;

        [FormerlySerializedAs("_expSlotS")]
        [SerializeField] private UIItemSlot expSlotS;
        [FormerlySerializedAs("_expSlotM")]
        [SerializeField] private UIItemSlot expSlotM;
        [FormerlySerializedAs("_expSlotL")]
        [SerializeField] private UIItemSlot expSlotL;

        [FormerlySerializedAs("_level")]
        [SerializeField] private UILevel level;

        [FormerlySerializedAs("_increase")]
        [SerializeField] private Button increase;
        [FormerlySerializedAs("_decrease")]
        [SerializeField] private Button decrease;
        [FormerlySerializedAs("_slider")]
        [SerializeField] private Slider slider;
        [FormerlySerializedAs("_quantity")]
        [SerializeField] private TMP_Text quantity;
        [FormerlySerializedAs("_minCount")]
        [SerializeField] private TMP_Text minCount;
        [FormerlySerializedAs("_maxCount")]
        [SerializeField] private TMP_Text maxCount;
        [FormerlySerializedAs("_gold")]
        [SerializeField] private TMP_Text gold;

        private readonly ReactiveProperty<UIItemSlot> _selected = new();

        protected override GrowthAction Action => GrowthAction.Lv;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(increase, () => slider.value = Mathf.Min(slider.value + 1, slider.maxValue));
            this.SubscribeButton(decrease, () => slider.value = Mathf.Min(slider.value - 1, slider.minValue));

            slider.onValueChanged
                .AsObservable()
                .Subscribe(OnSliderValueChanged)
                .AddTo(this);

            _selected
                .Subscribe(OnSlotSelected)
                .AddTo(this);

            _userRepo.Inventory
                .WhenUpdate(_ =>
                {
                    InitItemSlots();
                    _selected.Value = _selected.Value;
                });

            expSlotS.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            expSlotM.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            expSlotL.OnClick += (x) => _selected.Value = (UIItemSlot)x;
        }

        public override async UniTask Open(params object[] parameters)
        {
            gold.text = "0";

            int initialSlot = parameters.Length > 1 && parameters[1] is int id
                ? id
                : ExpSmallId;

            InitItemSlots(initialSlot);

            await base.Open(parameters);
        }

        private void InitItemSlots(int initialSlot = -1)
        {
            var s = GetExpItem(ExpSmallId);
            var m = GetExpItem(ExpMediumId);
            var l = GetExpItem(ExpLargeId);

            expSlotS.quantityDisplay.Value = UIItemSlot.QuantityDisplay.ValueOnly;
            expSlotM.quantityDisplay.Value = UIItemSlot.QuantityDisplay.ValueOnly;
            expSlotL.quantityDisplay.Value = UIItemSlot.QuantityDisplay.ValueOnly;

            expSlotS.Init(s);
            expSlotM.Init(m);
            expSlotL.Init(l);

            if (initialSlot == ExpSmallId)
                _selected.Value = expSlotS;

            else if (initialSlot == ExpMediumId)
                _selected.Value = expSlotM;

            else if (initialSlot == ExpLargeId)
                _selected.Value = expSlotL;

            if (_selected.Value == null)
                _selected.Value = expSlotS;
        }

        private IItem GetExpItem(int id)
        {
            return _userRepo.Inventory.Items.FirstOrDefault(x => x.ItemId == id)
                   ?? new Item { ItemId = id, };
        }

        protected override void OnUnitChanged(UnitInfo unit)
        {
            level.Set(unit);
            slider.value = Mathf.Min(_selected.Value.Item.Quantity, 1); ;
        }

        protected override (int id, int quantity) ConsumeItem()
        {
            var selected = _selected.Value;
            return selected != null
                ? (selected.Item.ItemId, (int)slider.value)
                : default;
        }

        private void OnSliderValueChanged(float value)
        {
            if (_unit.Value == null)
                return;

            var itemQty = Mathf.RoundToInt(value);
            if (itemQty == 0)
            {
                level.ReleaseOverride();
                gold.text = "0";
                quantity.text = $"0";
                return;
            }
            var unit = _unit.Value;
            UnitHelper.CalculateLvUp(unit.lv, unit.exp, _selected.Value.Entity, itemQty,
                out int lv, out int exp, out _, out int leftItem, out int price);

            bool hasEnoughGold = price <= _userRepo.Currency.Gold.Value;
            _goldSlot.QuantityLabelColor = hasEnoughGold
                ? Color.white
                : StringHelper.NegativeColor;

            gold.text = price.CurrencyText();
            level.SetOverride(lv, exp);
            _confirm.interactable = hasEnoughGold;

            // ���� ������ ���� WithoutNotify ���.
            int clamped = itemQty - leftItem;
            slider.SetValueWithoutNotify(itemQty - leftItem);
            quantity.text = $"{clamped:N0}";
        }

        private void OnSlotSelected(UIItemSlot slot)
        {
            if (slot == null)
                return;

            int minCountVal = Mathf.Min(slot.Item.Quantity, 1);
            slider.value = minCountVal;
            slider.maxValue = slot.Item.Quantity;
            quantity.text = $"{minCountVal:N0}";
            maxCount.text = $"{slider.maxValue:N0}";

            expSlotS.state.Value = expSlotS == slot
                ? UIState.State.Highlighted
                : default;

            expSlotM.state.Value = expSlotM == slot
                ? UIState.State.Highlighted
                : default;

            expSlotL.state.Value = expSlotL == slot
                ? UIState.State.Highlighted
                : default;
        }
    }
}
