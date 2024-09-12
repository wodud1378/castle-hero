using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_LevelUp.prefab")]
    public class PopupLevelUp : PopupGrowth
    {
        private const int ExpSmallId = 52001;
        private const int ExpMediumId = 52002;
        private const int ExpLargeId = 52003;

        [SerializeField] private UIItemSlot _expSlotS;
        [SerializeField] private UIItemSlot _expSlotM;
        [SerializeField] private UIItemSlot _expSlotL;

        [SerializeField] private UILevel _level;
       
        [SerializeField] private Button _increase;
        [SerializeField] private Button _decrease;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _quantity;
        [SerializeField] private TMP_Text _minCount;
        [SerializeField] private TMP_Text _maxCount;
        [SerializeField] private TMP_Text _gold;

        private readonly ReactiveProperty<UIItemSlot> _selected = new();

        protected override CharacterService.GrowthAction Action => CharacterService.GrowthAction.Lv;

        protected override void OnAwake()
        {
            base.OnAwake();
            
            this.SubscribeButton(_increase, ()=> _slider.value = Mathf.Min(_slider.value + 1, _slider.maxValue) );
            this.SubscribeButton(_decrease, ()=> _slider.value = Mathf.Min(_slider.value - 1, _slider.minValue) );

            _slider.onValueChanged
                .AsObservable()
                .Subscribe(OnSliderValueChanged)
                .AddTo(this);

            _selected
                .Subscribe(OnSlotSelected)
                .AddTo(this);

            Storage.userRepository.inventory
                .WhenUpdate(_ =>
                {
                    InitItemSlots()
                        .ContinueWith(() => _selected.Value = _selected.Value)
                        .Forget();
                });

            _expSlotS.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            _expSlotM.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            _expSlotL.OnClick += (x) => _selected.Value = (UIItemSlot)x;
        }

        public override async UniTask Open(params object[] parameters)
        {
            _gold.text = "0";

            int initialSlot = parameters.Length > 1 && parameters[1] is int id
                ? id
                : ExpSmallId;
            
            await InitItemSlots(initialSlot);

            await base.Open(parameters);
        }

        private async UniTask InitItemSlots(int initialSlot = -1)
        {
            var tasks = new UniTask[3];
            var s = GetExpItem(ExpSmallId);
            var m = GetExpItem(ExpMediumId);
            var l = GetExpItem(ExpLargeId);

            _expSlotS.quantityDisplay.Value = UIItemSlot.QuantityDisplay.ValueOnly;
            _expSlotM.quantityDisplay.Value = UIItemSlot.QuantityDisplay.ValueOnly;
            _expSlotL.quantityDisplay.Value = UIItemSlot.QuantityDisplay.ValueOnly;

            tasks[0] = _expSlotS.Init(s);
            tasks[1] = _expSlotM.Init(m);
            tasks[2] = _expSlotL.Init(l);

            await UniTask.WhenAll(tasks);

            if (initialSlot == ExpSmallId)
                _selected.Value = _expSlotS;
            
            else if (initialSlot == ExpMediumId)
                _selected.Value = _expSlotM;
            
            else if (initialSlot == ExpLargeId)
                _selected.Value = _expSlotL;

            if (_selected.Value == null)
                _selected.Value = _expSlotS;
        }

        private IItem GetExpItem(int id)
        {
            return Storage.userRepository.inventory.items.FirstOrDefault(x => x.ItemId == id)
                   ?? new Item { ItemId = id, };
        }

        protected override void OnUnitChanged(UnitInfo unit)
        {
            _level.Set(unit);
            _slider.value = Mathf.Min(_selected.Value.Item.Quantity, 1);;
        }

        protected override (int id, int quantity) ConsumeItem()
        {
            var selected = _selected.Value;
            return selected != null
                ? (selected.Item.ItemId, (int)_slider.value)
                : default;
        }

        private void OnSliderValueChanged(float value)
        {
            if (_unit.Value == null)
                return;

            var itemQty = Mathf.RoundToInt(value);
            if (itemQty == 0)
            {
                _level.ReleaseOverride();
                _gold.text = "0";
                _quantity.text = $"0";
                return;
            }
            var unit = _unit.Value;
            UnitHelper.CalculateLvUp(unit.lv, unit.exp, _selected.Value.Entity, itemQty,
                out int lv, out int exp, out _, out int leftItem, out int price);

            bool hasEnoughGold = price <= Storage.userRepository.currency.gold.Value;
            _goldSlot.QuantityLabelColor = hasEnoughGold
                ? Color.white
                : StringHelper.NegativeColor;

            _gold.text = price.CurrencyText();
            _level.SetOverride(lv, exp);
            _confirm.interactable = hasEnoughGold;

            // 루프 방지를 위해 WithoutNotify 사용.
            int clamped = itemQty - leftItem;
            _slider.SetValueWithoutNotify(itemQty - leftItem);
            _quantity.text = $"{clamped:N0}";
        }

        private void OnSlotSelected(UIItemSlot slot)
        {
            if (slot == null)
                return;

            int minCount = Mathf.Min(slot.Item.Quantity, 1);
            _slider.value = minCount;
            _slider.maxValue = slot.Item.Quantity;
            _quantity.text = $"{minCount:N0}";
            _maxCount.text = $"{_slider.maxValue:N0}";

            _expSlotS.state.Value = _expSlotS == slot
                ? UIState.State.Highlighted
                : default;

            _expSlotM.state.Value = _expSlotM == slot
                ? UIState.State.Highlighted
                : default;

            _expSlotL.state.Value = _expSlotL == slot
                ? UIState.State.Highlighted
                : default;
        }
    }
}