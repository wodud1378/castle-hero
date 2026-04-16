using System;
using System.Collections.Generic;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;
using UniRx;

namespace CastleHero.View.Lobby.UI.Inventory.Popup
{
    /// <summary>
    /// PopupInventory 의 Tab + Category 필터 상태와 필터링 로직 캡슐.
    /// PopupInventory 에서 toggle 바인딩은 유지하되, 필터링 판정과 컬렉션 상태는 이 타입으로 분리.
    /// </summary>
    public sealed class InventoryItemFilter
    {
        private readonly IDBProvider _db;

        public ReactiveProperty<PopupInventory.Tab> Tab { get; } = new();
        public ReactiveCollection<PopupInventory.Category> Categories { get; } = new();

        public InventoryItemFilter(IDBProvider db)
        {
            _db = db;
        }

        public IEnumerable<IItem> Apply(IEnumerable<IItem> source)
        {
            var compare = CompareMethod(Tab.Value);
            foreach (var item in source)
            {
                if (compare != null && !compare(item))
                    continue;

                if (Categories.Count > 0 && !MatchesCategory(item))
                    continue;

                yield return item;
            }
        }

        private Predicate<IItem> CompareMethod(PopupInventory.Tab tab)
        {
            return tab switch
            {
                PopupInventory.Tab.All => _ => true,
                PopupInventory.Tab.Equipment => x =>
                    _db.Items.TryFind(x.ItemId, out var entity) && entity.type == ItemType.Equipment,
                PopupInventory.Tab.Other => x =>
                    _db.Items.TryFind(x.ItemId, out var entity) && entity.type != ItemType.Equipment,
                _ => null
            };
        }

        private bool MatchesCategory(IItem item)
        {
            if (!_db.Items.TryFind(item.ItemId, out var entity))
                return false;

            var type = entity.type;
            if (type == ItemType.Equipment)
            {
                if (item is not EquipItem equipItem)
                    return false;

                var slot = (EquipmentSlot)equipItem.slot;
                return slot switch
                {
                    EquipmentSlot.Weapon => Categories.Contains(PopupInventory.Category.Weapon),
                    EquipmentSlot.Armor => Categories.Contains(PopupInventory.Category.Armor),
                    EquipmentSlot.Ring => Categories.Contains(PopupInventory.Category.Ring),
                    EquipmentSlot.Necklace => Categories.Contains(PopupInventory.Category.Necklace),
                    _ => false,
                };
            }

            return type switch
            {
                ItemType.Consumable => Categories.Contains(PopupInventory.Category.Consumable),
                ItemType.Ingredient => Categories.Contains(PopupInventory.Category.Ingredient),
                ItemType.Chest => Categories.Contains(PopupInventory.Category.Chest),
                _ => false,
            };
        }
    }
}
