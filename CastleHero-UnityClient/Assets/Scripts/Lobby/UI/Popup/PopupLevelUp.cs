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
    [PrefabPath("Lobby/UI/Prefabs/Popup_LevelUp.prefab")]
    public class PopupLevelUp : PopupGrowth
    {
        private const int ExpSmallId = 52001;
        private const int ExpMediumId = 52002;
        private const int ExpLargeId = 52003;

        [SerializeField] private UIItemSlot _expSlotS;
        [SerializeField] private UIItemSlot _expSlotM;
        [SerializeField] private UIItemSlot _expSlotL;

        [SerializeField] private UILevel _level;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _minCount;
        [SerializeField] private TMP_Text _maxCount;
        [SerializeField] private TMP_Text _gold;

        private readonly ReactiveProperty<UIItemSlot> _selected = new();

        protected override CharacterService.GrowthAction Action => CharacterService.GrowthAction.Lv;

        protected override void OnAwake()
        {
            base.OnAwake();

            _slider.onValueChanged
                .AsObservable()
                .Subscribe(OnSliderValueChanged)
                .AddTo(this);

            _selected
                .Subscribe(OnSlotSelected)
                .AddTo(this);
            
            Storage.userRepository.inventory.items
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(_ =>
                {
                    InitItemSlots()
                        .ContinueWith(()=> _selected.Value = _selected.Value)
                        .Forget();
                })
                .AddTo(this);

            _expSlotS.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            _expSlotM.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            _expSlotL.OnClick += (x) => _selected.Value = (UIItemSlot)x;
        }

        public override UniTask Open(params object[] parameters)
        {
            _gold.text = "0";

            return UniTask.WhenAll(base.Open(parameters), InitItemSlots());
        }

        private UniTask InitItemSlots()
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

            return UniTask.WhenAll(tasks);
        }

        private IItem GetExpItem(int id)
        {
            return Storage.userRepository.inventory.items.FirstOrDefault(x => x.ItemId == id)
                   ?? new Item { ItemId = id, };
        }

        protected override void OnUnitChanged(UnitInfo unit)
        {
            _level.Set(unit);

            _slider.value = 0;
            _selected.Value = _expSlotS;
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

            var count = Mathf.RoundToInt(value);
            if (count == 0)
            {
                _level.ReleaseOverride();
                _gold.text = "0";
                return;
            }

            var unit = _unit.Value;
            int itemQty = (int)_slider.value;
            UnitHelper.CalculateLvUp(unit.lv, unit.exp, _selected.Value.Entity, itemQty,
                out int lv, out int exp, out int leftItem, out int price);

            bool hasEnoughGold = price <= Storage.userRepository.currency.gold.Value;
            _goldSlot.QuantityLabelColor = hasEnoughGold 
                ? Color.white
                : StringHelper.NegativeColor;

            _gold.text = price.CurrencyText();
            _level.SetOverride(lv, exp);
            _confirm.interactable = hasEnoughGold;
            
            // 루프 방지를 위해 WithoutNotify 사용.
            _slider.SetValueWithoutNotify(itemQty - leftItem);
        }

        private void OnSlotSelected(UIItemSlot slot)
        {
            if (slot == null)
                return;

            _slider.value = 0;
            _slider.maxValue = slot.Item.Quantity;

            _maxCount.text = _slider.maxValue.ToString();
        }
    }
}