using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.View.Lobby.UI;
using CastleHero.Data.Model;
using CastleHero.View.Lobby.UI.Popup;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CastleHero.Common.Pattern;

using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Equipment.prefab")]

    public class PopupEquipItem : PopupItemBase<UIEquipmentSlot, EquipItem>, ISelect<EquipItem>
    {
        [FormerlySerializedAs("_mainStat")]
        [SerializeField] private UIStatusText[] mainStat;
        [FormerlySerializedAs("_stats")]
        [SerializeField] private UIStatusText[] stats;

        [FormerlySerializedAs("_refine")]
        [SerializeField] private Button refine;
        [FormerlySerializedAs("_equip")]
        [SerializeField] private Button equip;
        [FormerlySerializedAs("_release")]
        [SerializeField] private Button release;

        protected override int SellCount => 1;

        public UniTask<EquipItem> SelectTask => _ctSource.Task;

        private UniTaskCompletionSource<EquipItem> _ctSource;

        public void BeginSelect(bool _) => _ctSource = new UniTaskCompletionSource<EquipItem>();

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(refine, Refine);
            this.SubscribeButton(equip, OnClickEquip);
            this.SubscribeButton(release, Release);
        }

        protected override void SubscribeUpdate()
        {
            ServiceLocator.Get<IUserRepository>().Inventory
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

        private async UniTask Refine()
        {
            var items = ServiceLocator.Get<IUserRepository>().Inventory.Items
                .OfType<Item>()
                .Where(x => ServiceLocator.Get<IDBProvider>().Items.TryFind(x.ItemId, out var entity) &&
                            entity is { type: ItemType.Consumable, optionConsume: { type: ConsumeType.ElementalStone } });

            var selection = await ServiceLocator.Get<IPopupManager>().OpenAsync<PopupSelectItem>(items);

            bool closed = false;
            var entity = ServiceLocator.Get<IDBProvider>().Items.FallBackEntity();
            while (!closed && !entity.IsValid)
            {
                selection.BeginSelect(false);

                var selected = await selection.SelectTask;
                if (selected == null)
                    closed = true;
                else
                {
                    if (ServiceLocator.Get<IDBProvider>().Items.TryFind(selected.ItemId, out var e) &&
                        e is { type: ItemType.Consumable, optionConsume: { type: ConsumeType.ElementalStone } })
                    {
                        entity = e;
                    }
                }
            }

            if (closed)
                return;

            selection.Close();

            ServiceLocator.Get<IPopupManager>().Open<PopupRefine>(Item, entity);
        }

        protected override UniTask InitSlot(EquipItem item, UIEquipmentSlot slot) => slot.Init(item);

        private async UniTask OnClickEquip()
        {
            if (_ctSource != null)
            {
                _ctSource.TrySetResult(Item);
                _ctSource = null;
            }
            else
            {
                var popup = await ServiceLocator.Get<IPopupManager>().OpenAsync<PopupCharacterList>();
                popup.clickMethod = PopupCharacterList.ClickMethod.Equip;
                popup.equipParam.item = Item;
            }

            Close();
        }

        private async UniTask Release()
        {
            var unit = ServiceLocator.Get<IUserRepository>().Characters.Units.FirstOrDefault(x => x.id == Item.character);
            if (unit == null)
                return;

            var result = await ServiceLocator.Get<INetworkServiceProvider>().Character.Release(unit.id, Item.Guid);
            if (!result.IsSuccess)
            {
                ServiceLocator.Get<IPopupManager>().Open<PopupCommon>(result.error);
            }
        }

        protected override void OnDataChanged(EquipItem data)
        {
            base.OnDataChanged(data);

            bool onUse = data.character != 0;
            equip.gameObject.SetActive(!onUse);
            release.gameObject.SetActive(onUse);

            foreach (var label in mainStat)
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

            foreach (var label in stats)
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
