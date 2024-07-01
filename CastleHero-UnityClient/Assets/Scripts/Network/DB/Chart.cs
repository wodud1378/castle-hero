using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using LitJson;
using RGLabs.Data.DB;
using RGLabs.Data.Load;
using RGLabs.Data.Repositories;

namespace RGLabs.Network.DB
{
    public class Chart
    {
        private readonly CsvToDatabase _csvToDB = new();
        private readonly JsonToDatabase _jsonToDB = new();

        public async UniTask<DBCollections> LoadFromServer()
        {
            DBCollections collections = new();
            
            var tasks = new List<UniTask<(string chartName, Response reponse)>>();
            foreach (var chartInfo in await GetChartList())
            {
                var id = chartInfo.selectedChartFileId.ToString();
                var name = chartInfo.chartName;
                tasks.Add(BackendWrapper.GetChartContent(name, id));
            }

            var results = await UniTask.WhenAll(tasks);
            var map = results
                .Select(x => (x.chartName, x.reponse.raw.GetFlattenJSON()))
                .ToDictionary(x => x.chartName, y => y.Item2["rows"]);
            
            LoadInstance<StageDB>(map, x => collections.stages = x);
            LoadInstance<WaveDB>(map, x => collections.waves = x);
            LoadInstance<CastleDB>(map, x => collections.castles = x);
            LoadInstance<UnitDB>(map, x => collections.units = x);
            LoadInstance<UnitLevelDB>(map, x => collections.levels = x);
            LoadInstance<UnitRateDB>(map, x => collections.rates = x);
            LoadInstance<UnitBalanceDB>(map, x => collections.balances = x);
            LoadInstance<SkillDB>(map, x => collections.skills = x);
            LoadInstance<SummonDB>(map, x => collections.summons = x);
            LoadInstance<SummonGroupDB>(map, x => collections.summonGroups = x);

            EquipmentDB equipmentItems = null;
            LoadInstance<EquipmentDB>(map, x => equipmentItems = x);

            ConsumableDB consumableItems = null;
            LoadInstance<ConsumableDB>(map, x => consumableItems = x);

            IngredientDB ingredientItems = null;
            LoadInstance<IngredientDB>(map, x => ingredientItems = x);

            ChestDB chestItems = null;
            LoadInstance<ChestDB>(map, x => chestItems = x);

            collections.itemDBAccessor =
                new ItemDBAccessor(equipmentItems, consumableItems, ingredientItems, chestItems);
            
            collections.units.CacheUnitSizes();

            return collections;
        }

        public async UniTask<DBCollections> LoadFromLocal()
        {
            DBCollections collections = new();
            
            var tasks = new List<UniTask>
            {
                _csvToDB.Load<StageDB>(x => collections.stages = x),
                _csvToDB.Load<WaveDB>(x => collections.waves = x),
                _csvToDB.Load<UnitDB>(x => collections.units = x),
                _csvToDB.Load<UnitLevelDB>(x => collections.levels = x),
                _csvToDB.Load<UnitRateDB>(x => collections.rates = x),
                _csvToDB.Load<UnitBalanceDB>(x => collections.balances = x),
                _csvToDB.Load<SkillDB>(x => collections.skills = x, true),
                _csvToDB.Load<CastleDB>(x => collections.castles = x),
                _csvToDB.Load<SummonDB>(x => collections.summons = x),
                _csvToDB.Load<SummonGroupDB>(x => collections.summonGroups = x), 
            };

            EquipmentDB equipmentItems = null;
            tasks.Add(_csvToDB.Load<EquipmentDB>(x => equipmentItems = x));

            ConsumableDB consumableItems = null;
            tasks.Add(_csvToDB.Load<ConsumableDB>(x => consumableItems = x));

            IngredientDB ingredientItems = null;
            tasks.Add(_csvToDB.Load<IngredientDB>(x => ingredientItems = x));

            ChestDB chestItems = null;
            tasks.Add(_csvToDB.Load<ChestDB>(x => chestItems = x));

            await UniTask.WhenAll(tasks);

            collections.itemDBAccessor = new ItemDBAccessor(equipmentItems, consumableItems, ingredientItems, chestItems);
            collections.units.CacheUnitSizes();

            return collections;
        }

        private void LoadInstance<T>(Dictionary<string, JsonData> map, Action<T> onResult) where T : class, IDataBase
        {
            var type = typeof(T);
            var att = type.GetCustomAttribute<DBAttribute>();
            if (att == null)
                return;
            
            if (!map.TryGetValue(att.ChartName, out var json))
                return;
            
            onResult.Invoke(_jsonToDB.Convert<T>(json, type == typeof(SkillDB)));
        }

        private async UniTask<ChartInfo[]> GetChartList()
        {
            var response = await BackendWrapper.GetChartList();
            return response.data;
        }
    }
}