using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.Common.Pattern;
using CastleHero.View.Common;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using CastleHero.View.Lobby.UI.Adapter;
using CastleHero.View.Lobby.UI.Inventory.Actions;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CastleHero.View.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Inventory.prefab")]
    public class PopupInventory : PopupBase
    {
        public enum Mode
        {
            Default,
            Sell,
        }

        public enum Tab
        {
            All,
            Equipment,
            Other
        }

        public enum Category
        {
            All = 0,
            Weapon,
            Armor,
            Ring,
            Necklace,
            Ingredient,
            Consumable,
            Chest,
        }

        public Toggle all;
        public Toggle equipment;
        public Toggle other;
        public Toggle[] categoryToggles;

        [FormerlySerializedAs("_gold")]
        [SerializeField] private TMP_Text gold;
        [FormerlySerializedAs("_sell")]
        [SerializeField] private Button sell;
        [FormerlySerializedAs("_confirmSell")]
        [SerializeField] private Button confirmSell;
        [FormerlySerializedAs("_cancelSell")]
        [SerializeField] private Button cancelSell;
        [FormerlySerializedAs("_itemList")]
        [SerializeField] private UIInventoryItemList itemList;

        [Header("Tab Sprites")] [FormerlySerializedAs("_tabSprites")] [SerializeField]
        private SpriteState tabSprites;

        [Header("Category Colors")] [FormerlySerializedAs("_categoryColors")] [SerializeField]
        private ColorBlock categoryColors;

        private readonly ReactiveProperty<Mode> mode = new();

        private readonly Dictionary<Tab, Toggle> _tabToggles = new();
        private readonly Dictionary<Category, Toggle> _categoryToggles = new();

        private readonly List<UIItemSlot> _sellTargets = new();

        private InventoryItemFilter _filter;
        private InventoryItemActions _actions;
        private IDBProvider _db;
        private IPopupManager _popups;
        private IUserRepository _userRepo;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _db = sl.Get<IDBProvider>();
            _popups = sl.Get<IPopupManager>();
            _userRepo = sl.Get<IUserRepository>();
            _filter = new InventoryItemFilter(_db);
            _actions = sl.Get<InventoryItemActions>();

            itemList.OnSlotClickEvent -= OnClickItemSlot;
            itemList.OnSlotClickEvent += OnClickItemSlot;

            _userRepo.Currency.Gold
                .Subscribe(x => gold.text = x.CurrencyText())
                .AddTo(this);

            mode
                .Subscribe(OnModeChanged)
                .AddTo(this);

            BindTabToggle(Tab.All, all);
            BindTabToggle(Tab.Equipment, equipment);
            BindTabToggle(Tab.Other, other);

            for (var i = 0; i < categoryToggles.Length; i++)
                BindCategoryToggle((Category)i, categoryToggles[i]);

            _filter.Categories
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(UpdateTogglesStatus)
                .AddTo(this);

            _filter.Tab
                .Subscribe(UpdateTabsStatus)
                .AddTo(this);

            _filter.Tab.AsObservable()
                .Select(_ => UniRx.Unit.Default)
                .Merge(
                    _filter.Categories
                        .ChangeAsObservable()
                        .Select(_ => UniRx.Unit.Default),
                    _userRepo.Inventory.Items
                        .ChangeAsObservable()
                        .ThrottleFrame(1)
                        .Select(_ => UniRx.Unit.Default))
                .ThrottleFrame(1)
                .Subscribe(_ => UpdateList())
                .AddTo(this);

            this.SubscribeButton(sell, () => mode.Value = Mode.Sell);
            this.SubscribeButton(cancelSell, () => mode.Value = Mode.Default);
            this.SubscribeButton(confirmSell, Sell);
        }

        private async UniTask Sell()
        {
            if (mode.Value != Mode.Sell || _sellTargets.Count == 0)
                return;

            var selected = _sellTargets.Select(x => (x.Item, x.Item.Quantity)).ToArray();
            await _actions.Sell(selected);
        }

        private void OnModeChanged(Mode value)
        {
            sell.gameObject.SetActive(value == Mode.Default);
            cancelSell.gameObject.SetActive(value == Mode.Sell);
            confirmSell.gameObject.SetActive(value == Mode.Sell);

            switch (value)
            {
                case Mode.Default:
                    itemList.items.ForEach(x => x.state.Value = UIState.State.Default);
                    break;
                case Mode.Sell:
                    _sellTargets.Clear();
                    itemList.items.ForEach(x =>
                    {
                        x.state.Value = x.Entity.sellPrice <= 0
                            ? UIState.State.Dim
                            : UIState.State.Default;
                    });
                    break;
            }
        }

        private void OnClickItemSlot(UIItemSlot slot)
        {
            switch (mode.Value)
            {
                case Mode.Default:
                    OnClickSlotByDefault(slot);
                    break;
                case Mode.Sell:
                    RemoveOrAddSellTarget(slot);
                    break;
            }
        }

        private void RemoveOrAddSellTarget(UIItemSlot slot)
        {
            switch (slot.state.Value)
            {
                case UIState.State.Default:
                    slot.state.Value = UIState.State.Highlighted;
                    _sellTargets.Add(slot);
                    break;
                case UIState.State.Highlighted:
                    slot.state.Value = UIState.State.Default;
                    _sellTargets.Remove(slot);
                    break;
            }
        }

        private void OnClickSlotByDefault(UIItemSlot slot)
        {
            var item = slot.Item;
            var type = slot.Entity.type;
            switch (type)
            {
                case ItemType.Equipment:
                    if (slot.Item is EquipItem equipItem)
                        _actions.Equip(equipItem).SafeForget();
                    break;
                case ItemType.Consumable:
                case ItemType.Ingredient:
                case ItemType.Chest:
                    _popups.Open<PopupUseItem>(item);
                    break;
            }
        }

        private void UpdateTogglesStatus(ReactiveCollection<Category> categories)
        {
            foreach (var pair in _categoryToggles)
            {
                if (pair.Key == Category.All)
                    continue;

                var toggle = pair.Value;
                var color = categories.Contains(pair.Key) ? categoryColors.selectedColor : categoryColors.normalColor;
                toggle.image.CrossFadeColor(color, categoryColors.fadeDuration, true, true);
            }

            _categoryToggles[Category.All].image
                .CrossFadeColor(categories.Any()
                        ? categoryColors.normalColor
                        : categoryColors.selectedColor,
                    categoryColors.fadeDuration,
                    true, true);
        }

        private void UpdateTabsStatus(Tab tab)
        {
            foreach (var pair in _tabToggles)
            {
                var toggle = pair.Value;
                toggle.image.overrideSprite = tab == pair.Key ? tabSprites.selectedSprite : null;
            }

            UpdateTogglesActive(tab);
        }

        private void BindTabToggle(Tab tab, Toggle toggle)
        {
            toggle.onValueChanged
                .AsObservable()
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    if (x)
                        _filter.Tab.Value = tab;
                })
                .AddTo(toggle);

            _tabToggles.Add(tab, toggle);
        }

        private void BindCategoryToggle(Category category, Toggle toggle)
        {
            toggle.onValueChanged
                .AsObservable()
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    if (category == Category.All)
                        _filter.Categories.Clear();
                    else
                    {
                        if (x)
                        {
                            if (!_filter.Categories.Contains(category))
                                _filter.Categories.Add(category);
                        }
                        else
                        {
                            if (_filter.Categories.Contains(category))
                                _filter.Categories.Remove(category);
                        }
                    }
                })
                .AddTo(toggle);

            _categoryToggles.Add(category, toggle);
        }

        public override UniTask Open(params object[] parameters)
        {
            Tab tabParam;
            Category category;
            try { tabParam = (Tab)parameters[0]; } catch { tabParam = default; }
            try { category = (Category)parameters[1]; } catch { category = default; }

            _filter.Tab.Value = tabParam;

            if (category != Category.All)
                _filter.Categories.Add(category);
            else
                UpdateTogglesStatus(_filter.Categories);

            return UniTask.CompletedTask;
        }

        public override UniTask Open() => Open(Tab.All);

        private void UpdateList()
        {
            var items = _filter.Apply(_userRepo.Inventory.Items);
            itemList.Init(items);
        }

        private void UpdateTogglesActive(Tab val)
        {
            if (val == Tab.All)
            {
                foreach (var toggle in _categoryToggles)
                    toggle.Value.gameObject.SetActive(true);
                return;
            }

            foreach (var toggle in _categoryToggles)
            {
                bool isActive;
                switch (toggle.Key)
                {
                    case Category.Weapon:
                    case Category.Armor:
                    case Category.Ring:
                    case Category.Necklace:
                        isActive = val == Tab.Equipment;
                        break;
                    case Category.Ingredient:
                    case Category.Consumable:
                    case Category.Chest:
                        isActive = val == Tab.Other;
                        break;
                    default:
                        isActive = true;
                        break;
                }

                toggle.Value.gameObject.SetActive(isActive);
            }
        }
    }
}
