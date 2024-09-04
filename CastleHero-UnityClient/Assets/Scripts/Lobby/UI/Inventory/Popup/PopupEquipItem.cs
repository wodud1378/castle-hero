using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Equipment.prefab")]

    public class PopupEquipItem : PopupItemBase<UIEquipmentSlot, EquipItem>, ISelect<EquipItem>
    {
        [SerializeField] private UIStatusText[] _mainStat; 
        [SerializeField] private UIStatusText[] _stats;

        [SerializeField] private Button _refine;
        [SerializeField] private Button _equip;
        [SerializeField] private Button _release;

        public UniTask<EquipItem> SelectTask => _ctSource.Task;
        
        private UniTaskCompletionSource<EquipItem> _ctSource;

        public void BeginSelect(bool _) => _ctSource = new UniTaskCompletionSource<EquipItem>();

        protected override void OnAwake()
        {
            base.OnAwake();
            
            this.SubscribeButton(_refine, Refine);
            this.SubscribeButton(_equip, OnClickEquip);
            this.SubscribeButton(_release, Release);
        }

        protected override void SubscribeUpdate()
        {
            Storage.userRepository.inventory
                .WhenUpdate(WhenUpdateItems)
                .AddTo(this);
        }

        protected override void OnClose()
        {
            base.OnClose();

            if (_ctSource != null)
            {
                _ctSource.TrySetResult(null);
                _ctSource = null;
            }
        }

        private void WhenUpdateItems(ReactiveCollection<IItem> items)
        {
            var exist = item.Value;
            if (exist == null)
                return;
            
            item.Value = items.OfType<EquipItem>().FirstOrDefault(x => x.Guid == exist.Guid);
        }

        private async void Refine()
        {
            if (!Context.popups.TryGetPopupIfExist(out PopupInventory inventory))
                inventory = await Context.popups.OpenAsync<PopupInventory>();
            else
                Context.popups.ReplaceToTop(inventory);
            
            inventory.filter.Clear();
            inventory.customFilter.Value = (_, entity) 
                => entity is { type: ItemType.Consumable, optionConsume: { type: ConsumeType.ElementalStone } };

            bool closed = false;
            var entity = Storage.db.items.FallBackEntity();
            while (!closed && !entity.IsValid)
            {
                inventory.BeginSelect(false);

                var selected = await inventory.SelectTask;
                if (selected == null)
                    closed = true;
                else
                {
                    if (Storage.db.items.TryFind(selected.ItemId, out var e) &&
                        e is { type: ItemType.Consumable, optionConsume: { type: ConsumeType.ElementalStone } })
                    {
                        entity = e;
                    }
                }
            }

            if (closed)
                return;
            
            Context.popups.Open<PopupRefine>(Item, entity);
        }

        protected override UniTask InitSlot(EquipItem item, UIEquipmentSlot slot) => slot.Init(item);

        private async void OnClickEquip()
        {
            if (_ctSource != null)
            {
                _ctSource.TrySetResult(Item);
                _ctSource = null;
            }
            else
            {
                var popup = await Context.popups.OpenAsync<PopupCharacterList>();
                popup.clickMethod = PopupCharacterList.ClickMethod.Equip;
                popup.equipParam.item = Item;   
            }
            
            Close();
        }

        private async void Release()
        {
            var unit = Storage.userRepository.characters.units.FirstOrDefault(x => x.id == Item.character);
            if (unit == null)
                return;

            var result = await NetworkService.Character.Release(unit.id, Item.Guid);
            if (!result.IsSuccess)
            {
                Context.popups.Open<PopupCommon>(result.error);
            }
        }

        protected override void OnDataChanged(EquipItem data)
        {
            base.OnDataChanged(data);

            bool onUse = data.character != 0;
            _equip.gameObject.SetActive(!onUse);
            _release.gameObject.SetActive(onUse);
            
            foreach (var label in _mainStat)
            {
                var main = data.main;
                bool matches = (int)label.type == main.type;
                if (matches)
                {
                    label.SetText(main.value);
                    label.gameObject.SetActive(true);
                }
                else
                {
                    label.gameObject.SetActive(false);
                }
            }
            
            foreach (var label in _stats)
            {
                var type = (int)label.type;
                
                var stat = data.sub.Find(x => x.type == type);
                if(stat != null)
                {
                    label.SetText(stat.value);
                    label.gameObject.SetActive(true);
                }
                else 
                    label.gameObject.SetActive(false);
            }
        }
    }
}