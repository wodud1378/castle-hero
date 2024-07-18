using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
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
    public class PopupLevelUp : PopupBase
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

        [SerializeField] private Button _confirm;

        private readonly ReactiveProperty<UnitInfo> _unit = new();
        private readonly ReactiveProperty<UIItemSlot> _selected = new();

        private readonly CharacterService _service = new();

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

            _unit
                .Subscribe(OnUnitChanged)
                .AddTo(this);

            _expSlotS.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            _expSlotM.OnClick += (x) => _selected.Value = (UIItemSlot)x;
            _expSlotL.OnClick += (x) => _selected.Value = (UIItemSlot)x;

            this.SubscribeButton(_confirm, ()=> Confirm().Forget());
        }

        public override async UniTask Open(params object[] parameters)
        {
            await InitItemSlots();

            if (parameters.Length > 0 && parameters[0] is UnitInfo unit)
                _unit.Value = unit;

            _gold.text = "0";
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
            return Storage.userRepository.items.FirstOrDefault(x => x.ItemId == id)
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

            Calculate(_unit.Value.lv, out int lv, out int exp, out int maxExp, out int gold);

            _gold.text = gold.CurrencyText();
            _maxCount.text = count.ToString();
            _level.SetOverride(lv, exp);
        }

        private void Calculate(int startLv, out int endLv, out int endExp, out int maxExp, out int requireGold)
        {
            endLv = 0;
            endExp = 0;
            maxExp = 0;
            requireGold = 0;

            var option = _selected.Value.Entity.GetConsumableOption();

            endExp = (int)(_slider.value * option.value);
            var db = Storage.db.levels;
            int lv = startLv;
            while (db.TryFind(lv++, out var entity) && endExp - entity.exp > 0)
            {
                endLv = entity.Id + 1;
                maxExp = entity.exp;
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
        }

        private async UniTaskVoid Confirm()
        {
            if (_selected.Value.Item is not Item item)
                return;
            
            var result = await _service.LevelUp(_unit.Value.id, item.ItemId, (int)_slider.value);
            _unit.Value = result.unit;
            _selected.Value.Init(result.leftItem);
        }
    }
}