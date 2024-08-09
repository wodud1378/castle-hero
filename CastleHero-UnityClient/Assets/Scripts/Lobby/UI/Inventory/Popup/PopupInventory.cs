using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Adapter;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Inventory.prefab")]
    public class PopupInventory : PopupBase
    {
        public enum Mode
        {
            Default,
            Sell
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

        [SerializeField] private TMP_Text _sellGold;
        [SerializeField] private Button _sell;
        [SerializeField] private UIInventoryItemList _itemList;

        [Header("Tab Sprites")]
        [SerializeField] private SpriteState _tabSprites;
        [Header("Category Colors")]
        [SerializeField] private ColorBlock _categoryColors;
        
        public readonly ReactiveProperty<Tab> tab = new();
        public readonly ReactiveCollection<Category> filter = new();

        private readonly Dictionary<Tab, Toggle> _tabToggles = new();
        private readonly Dictionary<Category, Toggle> _categoryToggles = new();
        
        private readonly ReactiveProperty<Mode> _mode = new();
        private readonly List<UIItemSlot> _sellTargets = new();

        private UniTask _updateTask;

        protected override void OnAwake()
        {
            base.OnAwake();

            _itemList.OnSlotClickEvent -= OnClickItemSlot;
            _itemList.OnSlotClickEvent += OnClickItemSlot;
            _mode
                .Subscribe(_=> _itemList.items.ForEach(x=>x.state.Value = UISlot.State.Default))
                .AddTo(this);
            
            BindTabToggle(Tab.All, all);
            BindTabToggle(Tab.Equipment, equipment);
            BindTabToggle(Tab.Other, other);

            for (var i = 0; i < categoryToggles.Length; i++)
            {
                BindCategoryToggle((Category)i, categoryToggles[i]);
            }
            
            filter
                .ChangeAsObservable()
                .Subscribe(UpdateTogglesStatus)
                .AddTo(this);

            tab
                .Subscribe(UpdateTabsStatus)
                .AddTo(this);

            tab.AsObservable()
                .Select(_ => UniRx.Unit.Default)
                .Merge(
                    filter
                        .ChangeAsObservable()
                        .Select(_ => UniRx.Unit.Default), 
                    
                    Storage.userRepository.inventory.items
                        .ChangeAsObservable()
                        .Select(_=> UniRx.Unit.Default))
                .ThrottleFrame(1)
                .Subscribe(_ => UpdateList())
                .AddTo(this);
        }

        private void OnModeChanged(Mode value)
        {
            if (value == Mode.Default)
                _itemList.items.ForEach(x => x.state.Value = UISlot.State.Default);

            else
            {
                _sellTargets.Clear();
                _itemList.items.ForEach(x =>
                {
                    x.state.Value = x.Entity.sellPrice > 0
                        ? UISlot.State.Diminished
                        : UISlot.State.Default;
                });
            }
        }
        
        private void OnClickItemSlot(UIItemSlot slot)
        {
            switch (_mode.Value)
            {
                case Mode.Default:
                    OpenPopup(slot);
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
                case UISlot.State.Default:
                    slot.state.Value = UISlot.State.Highlighted;
                    _sellTargets.Add(slot);
                    break;
                case UISlot.State.Highlighted:
                    slot.state.Value = UISlot.State.Default;
                    _sellTargets.Remove(slot);
                    break;
            }
        }

        private void OpenPopup(UIItemSlot slot)
        {
            var item = slot.Item;
            var type = slot.Entity.type;
            switch (type)
            {
                case ItemType.Equipment:
                    Context.popupManager.OpenAsync<PopupEquipItem>(item).Forget();
                    break;
                case ItemType.Consumable:
                case ItemType.Ingredient:
                case ItemType.Chest:
                    Context.popupManager.OpenAsync<PopupUseItem>(item).Forget();
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
                var color = categories.Contains(pair.Key)? _categoryColors.selectedColor : _categoryColors.normalColor;
                toggle.image.CrossFadeColor(color, _categoryColors.fadeDuration, true, true);
            }
                    
            _categoryToggles[Category.All].image
                .CrossFadeColor(categories.Any() 
                        ? _categoryColors.normalColor
                        : _categoryColors.selectedColor,
                    _categoryColors.fadeDuration,
                    true, true);
        }

        private void UpdateTabsStatus(Tab tab)
        {
            foreach (var pair in _tabToggles)
            {
                var toggle = pair.Value;
                toggle.image.overrideSprite = tab == pair.Key ? _tabSprites.selectedSprite : null;
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
                        this.tab.Value = tab;
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
                        filter.Clear();
                    else
                    {
                        if (x)
                        {
                            if (!filter.Contains(category))
                                filter.Add(category);
                        }
                        else
                        {
                            if (filter.Contains(category))
                                filter.Remove(category);
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
            try
            {
                tabParam = (Tab)parameters[0];
            }
            catch
            {
                tabParam = default;
            }

            try
            {
                category = (Category)parameters[1];
            }
            catch
            {
                category = default;
            }

            tab.Value = tabParam;
            
            if(category != Category.All)
                filter.Add(category);
            else
                UpdateTogglesStatus(filter);

            return _updateTask;
        }

        public override UniTask Open() => Open(Tab.All);

        private void UpdateList()
        {
            var items = Storage.userRepository.inventory.items
                .Where(CompareMethod(tab.Value).Invoke);

            if(filter.Count > 0)
                items = items.Where(Filter);
            
            _updateTask = _itemList.Init(items);
        }
        
        private void UpdateTogglesActive(Tab val)
        {
            if (val == Tab.All)
            {
                foreach (var toggle in _categoryToggles)
                {
                    toggle.Value.gameObject.SetActive(true);
                }

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

        private bool Filter(IItem item)
        {
            if (!Storage.db.items.TryFind(item.ItemId, out var entity))
            {
                return false;
            }

            var type = entity.type;
            if (type == ItemType.Equipment)
            {
                if (item is EquipItem equipItem)
                {
                    var slot = (EquipmentSlot)equipItem.slot;
                    switch (slot)
                    {
                        case EquipmentSlot.Weapon:
                            return filter.Contains(Category.Weapon);
                        case EquipmentSlot.Armor:
                            return filter.Contains(Category.Armor);
                        case EquipmentSlot.Ring:
                            return filter.Contains(Category.Ring);
                        case EquipmentSlot.Necklace:
                            return filter.Contains(Category.Necklace);
                    }
                }
                else
                    return false;
            }

            switch (type)
            {
                case ItemType.Consumable:
                    return filter.Contains(Category.Consumable);

                case ItemType.Ingredient:
                    return filter.Contains(Category.Ingredient);

                case ItemType.Chest:
                    return filter.Contains(Category.Chest);
            }

            return false;
        }

        private Predicate<IItem> CompareMethod(Tab tabValue)
        {
            return tabValue switch
            {
                Tab.All => _ => true,
                Tab.Equipment => x=> Storage.db.items.TryFind(x.ItemId, out var entity) && entity.type == ItemType.Equipment,
                Tab.Other => x => Storage.db.items.TryFind(x.ItemId, out var entity) && entity.type != ItemType.Equipment,
                _ => null
            };
        }
    }
}