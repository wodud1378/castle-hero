using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Data.DB;
using RGLabs.Data.Load;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
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
            var values = Enum.GetValues(typeof(ItemTypeCode));
            bool found = false;
            ItemTypeCode code = ItemTypeCode.Equipment;
            foreach (ItemTypeCode value in values)
            {
                int sub = id - (int)value;
                found = sub is > 0 and < 10000;

                if (found)
                    code = value;
            }

            if (found)
            {
                switch (code)
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
            }

            entity = default;
            return false;
        }
    }

    public class DBCollections
    {
        public static async UniTask<DBCollections> Load()
        {
            if (_loaded == null)
            {
                _loaded = new DBCollections();

                await _loaded.Init();
            }

            return _loaded;
        }

        private static DBCollections _loaded;

        public StageDB stages;
        public WaveDB waves;

        public ItemDBAccessor itemDBAccessor;

        public UnitDB units;
        public UnitLevelDB levels;
        public SkillDB skills;

        private readonly DataLoader _loader;

        private DBCollections()
        {
            _loader = new DataLoader(new LocalCsvProvider());
        }

        private async UniTask Init()
        {
            var tasks = new List<UniTask>
            {
                _loader.Load<StageDB>(x => stages = x),
                _loader.Load<WaveDB>(x => waves = x),
                _loader.Load<UnitDB>(x => units = x),
                _loader.Load<UnitLevelDB>(x => levels = x),
                _loader.Load<SkillDB>(x => skills = x)
            };

            EquipmentDB equipmentItems = null;
            tasks.Add(_loader.Load<EquipmentDB>(x => equipmentItems = x));

            ConsumableDB consumableItems = null;
            tasks.Add(_loader.Load<ConsumableDB>(x => consumableItems = x));

            IngredientDB ingredientItems = null;
            tasks.Add(_loader.Load<IngredientDB>(x => ingredientItems = x));

            ChestDB chestItems = null;
            tasks.Add(_loader.Load<ChestDB>(x => chestItems = x));

            await UniTask.WhenAll(tasks);

            itemDBAccessor = new ItemDBAccessor(equipmentItems, consumableItems, ingredientItems, chestItems);

            units.CacheUnitSizes();
        }
    }

    public class InGameRepository : IDisposable
    {
        public readonly ReactiveProperty<UnitBehaviour> castle = new(null);
        public readonly ReactiveProperty<UnitBehaviour[]> characters = new(null);

        public readonly ReactiveCollection<WaitRecover> recovers = new();

        public void Dispose()
        {
            castle.Value = null;
            characters.Value = null;
            recovers.Clear();

            castle.Dispose();
            characters.Dispose();
        }
    }
}