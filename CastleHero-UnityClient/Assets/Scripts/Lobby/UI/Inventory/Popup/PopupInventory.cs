using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
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

        [Flags]
        public enum Category
        {
            All = 0,
            Weapon = 1 << 0,
            Armor = 1 << 1,
            Ring = 1 << 2,
            Necklace = 1 << 3,
            Ingredient = 1 << 4,
            Consumable = 1 << 5,
            Chest = 1 << 6,
        }

        [SerializeField] private UIInventoryItemList _itemList;
        [SerializeField] private ToggleGroup _tabToggle;

        [SerializeField] private Toggle[] _categoryToggles;

        public readonly ReactiveProperty<Tab> tab = new();
        public readonly ReactiveProperty<Category> category = new();

        private Dictionary<Category, Toggle> _toggles;

        private UniTask _updateTask;
        
        protected override void InitSubscriptions()
        {
            base.InitSubscriptions();

            _toggles = new();
            for(int i = 0; i < _categoryToggles.Length; ++i)
            {
                var toggle = _categoryToggles[i];
                Category target = (Category)i;
                toggle.onValueChanged
                    .AsObservable()
                    .Subscribe(x=>
                    {
                        if (target == Category.All)
                            category.Value = Category.All;
                        
                        if (x)
                            category.Value |= target;
                        else
                            category.Value &= ~ target;
                    })
                    .AddTo(this);

                _toggles.TryAdd(target, toggle);
            }
            
            this.UpdateAsObservable()
                .Select(_ => _tabToggle.ActiveToggles().FirstOrDefault(t => t.isOn))
                .DistinctUntilChanged()
                .Select(toggle => Enum.Parse<Tab>(toggle.gameObject.name))
                .Subscribe(selected => tab.Value = selected)
                .AddTo(this);
            
            var itemObservable = Storage.userRepository.items.ChangeAsObservable();
            tab.CombineLatest(category, itemObservable, (t, c, i) => (t, c, i))
                .ThrottleFrame(1)
                .Subscribe(_=> UpdateList())
                .AddTo(this);

            tab
                .Subscribe(UpdateTogglesActive)
                .AddTo(this);
        }

        public override UniTask Open(params object[] parameters)
        {
            Tab tabParam;
            Category categoryParam;
            try { tabParam = (Tab)parameters[0]; }
            catch { tabParam = default; }

            try { categoryParam = (Category)parameters[1]; }
            catch { categoryParam = default; }

            tab.Value = tabParam;
            category.Value = categoryParam;

            return _updateTask;
        }

        public override UniTask Open() => Open(Tab.All);

        private void UpdateList()
        {
            var items = Storage.userRepository.items
                .Where(CompareMethod(tab.Value).Invoke);

            if (category.Value != Category.All)
                items = items.Where(Filter);

            _updateTask = _itemList.Init(items);
        }

        private void UpdateTogglesActive(Tab val)
        {
            if (val == Tab.All)
            {
                foreach (var toggle in _toggles)
                {
                    toggle.Value.gameObject.SetActive(true);
                }
                
                return;
            }
            
            foreach (var toggle in _toggles)
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
                            return (category.Value & Category.Weapon) != 0;
                        case EquipmentSlot.Armor:
                            return (category.Value & Category.Armor) != 0;
                        case EquipmentSlot.Ring:
                            return (category.Value & Category.Ring) != 0;
                        case EquipmentSlot.Necklace:
                            return (category.Value & Category.Necklace) != 0;
                    }
                }
                else
                    return false;
            }
            
            switch (item.Id.ItemType())
            {    
                case ItemTypeCode.Consumable:
                    return (category.Value & Category.Consumable) != 0;

                case ItemTypeCode.Ingredient:
                    return (category.Value & Category.Ingredient) != 0;

                case ItemTypeCode.Chest:
                    return (category.Value & Category.Chest) != 0;
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