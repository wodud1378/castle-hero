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
        private readonly DataLoader _loader;

        public StageDB stages;
        public WaveDB waves;

        public ItemDBAccessor itemDBAccessor;

        public CastleDB castles;
        public UnitDB units;

        public UnitLevelDB levels;
        public UnitRateDB rates;
        public UnitBalanceDB balances;
        public SkillDB skills;

        public SummonDB summons;
        public SummonGroupDB summonGroups;

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
                _loader.Load<UnitRateDB>(x => rates = x),
                _loader.Load<UnitBalanceDB>(x => balances = x),
                _loader.Load<SkillDB>(x => skills = x, true),
                _loader.Load<CastleDB>(x => castles = x),
                _loader.Load<SummonDB>(x => summons = x),
                _loader.Load<SummonGroupDB>(x => summonGroups = x),
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
        public readonly ReactiveProperty<int> mana = new(0);
        public readonly ReactiveProperty<UnitBehaviour> castle = new(null);
        public readonly ReactiveCollection<UnitBehaviour> characters = new();
        public readonly ReactiveCollection<WaitRecover> recovers = new();

        public void Dispose()
        {
            castle.Value = null;
            characters.Dispose();
            characters.Clear();
            ;
            recovers.Dispose();
            recovers.Clear();

            castle.Dispose();
            characters.Dispose();
        }
    }
}