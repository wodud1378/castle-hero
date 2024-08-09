using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Network.Shared;
using RGLabs.Network.Service.Character;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_LevelUp.prefab")]
    public class PopupLevelUp : PopupBase, IGrowthTask
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

        [SerializeField] private UIItemSlot _goldSlot;
        [SerializeField] private Button _confirm;

        public UniTask<GrowthResult> GrowthTask => _completionSource.Task;

        private UniTaskCompletionSource<GrowthResult> _completionSource;
        private GrowthResult _result;

        private readonly ReactiveProperty<UnitInfo> _unit = new();
        private readonly ReactiveProperty<UIItemSlot> _selected = new();

        private readonly CharacterService _service = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            Storage.userRepository.currency.gold
                .Subscribe(UpdateGoldSlot)
                .AddTo(this);

            _slider.onValueChanged
                .AsObservable()
                .Subscribe(OnSliderValueChanged)
                .AddTo(this);

            _selected
                .Subscribe(OnSlotSelected)
                .AddTo(this);

            _unit
                .Subscribe(OnUnitChanged)
                .AddTo(this);

            _expSlotS.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            _expSlotM.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            _expSlotL.OnClick += (x) => _selected.Value = (UIItemSlot)x;

            this.SubscribeButton(_confirm, () => Confirm().Forget());
        }

        public override async UniTask Open(params object[] parameters)
        {
            _completionSource = new();
            await InitItemSlots();

            if (parameters.Length > 0 && parameters[0] is UnitInfo unit)
                _unit.Value = unit;

            _gold.text = "0";
        }

        protected override void OnClose()
        {
            base.OnClose();

            _completionSource.TrySetResult(_result);
        }

        private void UpdateGoldSlot(int gold)
        {
            _goldSlot.Init(new Item
            {
                ItemId = Constants.GoldId,
                Quantity = gold
            });
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

        private void OnUnitChanged(UnitInfo unit)
        {
            if (unit == null)
                return;

            _level.Set(unit);

            _slider.value = 0;
            _selected.Value = _expSlotS;
        }

        private void OnSliderValueChanged(float value)
        {
            if (_unit.Value == null)
                return;

            var count = Mathf.RoundToInt(value);
            if (count == 0)
            {
                _level.ReleaseOverride();
                return;
            }

            var unit = _unit.Value;
            Calculate(unit.lv, unit.exp, out int lv, out int exp, out int gold);

            bool hasEnoughGold = gold <= Storage.userRepository.currency.gold.Value;
            _goldSlot.QuantityLabelColor = hasEnoughGold 
                ? Color.white
                : StringHelper.NegativeColor;

            _gold.text = gold.CurrencyText();
            _level.SetOverride(lv, exp);
            _confirm.interactable = hasEnoughGold;
        }

        private void Calculate(int startLv, int startExp, out int endLv, out int endExp, out int requireGold)
        {
            endLv = startLv;
            endExp = startExp;
            requireGold = 0;

            var option = _selected.Value.Entity.GetConsumableOption();

            endExp = (int)(_slider.value * option.value);
            var db = Storage.db.levels;
            int lv = startLv;
            while (db.TryFind(lv++, out var entity) && endExp - entity.exp > 0)
            {
                endLv = entity.Id + 1;
                requireGold += entity.gold;

                endExp -= entity.exp;
            }
        }

        private void OnSlotSelected(UIItemSlot slot)
        {
            if (slot == null)
                return;

            _slider.value = 0;
            _slider.maxValue = slot.Item.Quantity;

            _maxCount.text = _slider.maxValue.ToString();
        }

        private async UniTaskVoid Confirm()
        {
            if (_selected.Value.Item is not Item item)
                return;

            var result = await _service.LevelUp(_unit.Value.id, item.ItemId, (int)_slider.value);
            var unit = result.transition.unit;

            var slot = new List<UIItemSlot>
            {
                _expSlotS,
                _expSlotM,
                _expSlotL
            }.FirstOrDefault(x => x.Item.ItemId == result.leftItem.ItemId)!;

            slot.Init(result.leftItem)
                .Forget();

            var repository = Storage.userRepository;
            repository.currency.Update(result.leftCurrency);
            repository.inventory.Update(result.leftItem);
            repository.characters.Update(unit);

            _unit.Value = unit;
            _selected.Value = slot;
            _result = result;
        }
    }
}