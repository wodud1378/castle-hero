using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Data.DB;
using RGLabs.Data.Load;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;

namespace RGLabs.Data.Repositories
{
    public class ItemDBAccessor
    {
        private readonly EquipmentDB _equipmentItems;
        private readonly ConsumableDB _consumableItems;
        private readonly IngredientDB _ingredientItems;
        private readonly ChestDB _chestItems;

        public ItemDBAccessor(EquipmentDB equipmentItems, ConsumableDB consumableItems, IngredientDB ingredientItems,
            ChestDB chestItems)
        {
            _equipmentItems = equipmentItems;
            _consumableItems = consumableItems;
            _ingredientItems = ingredientItems;
            _chestItems = chestItems;
        }

        public bool TryLoad(int id, out IItemEntity entity)
        {
            switch (id.ItemType())
            {
                case ItemTypeCode.Equipment:
                    if (_equipmentItems.TryFind(id, out var equipment))
                    {
                        entity = equipment;
                        return true;
                    }

                    break;
                case ItemTypeCode.Consumable:
                    if (_consumableItems.TryFind(id, out var consumable))
                    {
                        entity = consumable;
                        return true;
                    }

                    break;
                case ItemTypeCode.Ingredient:
                    if (_ingredientItems.TryFind(id, out var ingredient))
                    {
                        entity = ingredient;
                        return true;
                    }

                    break;
                case ItemTypeCode.Chest:
                    if (_chestItems.TryFind(id, out var chest))
                    {
                        entity = chest;
                        return true;
                    }

                    break;
            }

            entity = null;
            return false;
        }
    }

    public class InGameRepository : IDisposable
    {
        public readonly ReactiveProperty<int> mana = new(0);
        public readonly ReactiveProperty<UnitBehaviour> castle = new(null);
        public readonly ReactiveCollection<UnitBehaviour> characters = new();
        public readonly ReactiveCollection<WaitRecover> recovers = new();

        public void Dispose()
        {
            castle.Value = null;
            characters.Dispose();
            characters.Clear();
            
            recovers.Dispose();
            recovers.Clear();

            castle.Dispose();
            characters.Dispose();
        }
    }
}