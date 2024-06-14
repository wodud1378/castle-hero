using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Inventory.prefab")]
    public class PopupInventory : PopupBase
    {
        public enum ClickMethod
        {
            Default,
            Refine,
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
        
        [SerializeField] private UIInventoryItemList _itemList;

        [Header("Tab Sprites")]
        [SerializeField] private SpriteState _tabSprites;
        [Header("Category Colors")]
        [SerializeField] private ColorBlock _categoryColors;
  
        public readonly ReactiveProperty<Tab> tab = new();
        public readonly ReactiveCollection<Category> filter = new();

        private readonly Dictionary<Tab, Toggle> _tabToggles = new();
        private readonly Dictionary<Category, Toggle> _categoryToggles = new();

        private UniTask _updateTask;

        protected override void InitSubscriptions()
        {
            base.InitSubscriptions();

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
                .Merge(filter.ChangeAsObservable().Select(_ => UniRx.Unit.Default))
                .Subscribe(_ => UpdateList())
                .AddTo(this);
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
            var items = Storage.userRepository.items
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
            var type = item.Id.ItemType();
            if (type == ItemTypeCode.Equipment)
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

            switch (item.Id.ItemType())
            {
                case ItemTypeCode.Consumable:
                    return filter.Contains(Category.Consumable);

                case ItemTypeCode.Ingredient:
                    return filter.Contains(Category.Ingredient);

                case ItemTypeCode.Chest:
                    return filter.Contains(Category.Chest);
            }

            return false;
        }

        private Predicate<IItem> CompareMethod(Tab tabValue)
        {
            return tabValue switch
            {
                Tab.All => _ => true,
                Tab.Equipment => x => x.Id.ItemType() == ItemTypeCode.Equipment,
                Tab.Other => x => x.Id.ItemType() != ItemTypeCode.Equipment,
                _ => null
            };
        }
    }
}