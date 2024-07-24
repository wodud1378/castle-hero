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
            
            var tasks = new List<UniTask<(string chartName, int id, Response response)>>();
            foreach (var chartInfo in await GetChartList())
            {
                var name = chartInfo.chartName;
                var id = chartInfo.selectedChartFileId;
                tasks.Add(GetChartContent(name, id));
            }
            
            var results = await UniTask.WhenAll(tasks);
            var map = results
                .ToDictionary(x => x.chartName, y => (y.id, y.response.raw.GetFlattenJSON()));
            
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
            LoadInstance<ItemDB>(map, x => collections.items = x);
            LoadInstance<EquipItemStatDB>(map, x => collections.stats = x);
            
            collections.units.CacheUnitSizes();

            return collections;
        }

        private async UniTask<(string chartName, int id, Response response)> GetChartContent(string chartName, int id) => (chartName, id, await BackendWrapper.GetChartContent(id.ToString()));

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
                _csvToDB.Load<ItemDB>(x => collections.items = x),
            };

            collections.units.CacheUnitSizes();

            return collections;
        }

        private void LoadInstance<T>(Dictionary<string, (int id, JsonData json)> map, Action<T> onResult) where T : class, IDataBase
        {
            var type = typeof(T);
            var att = type.GetCustomAttribute<DBAttribute>();
            if (att == null)
                return;
            
            if (!map.TryGetValue(att.ChartName, out var data))
                return;

            var obj = _jsonToDB.Convert<T>(data.json, type == typeof(SkillDB));
            obj.Id = data.id;
            onResult.Invoke(obj);
        }

        private async UniTask<ChartInfo[]> GetChartList()
        {
            var response = await BackendWrapper.GetChartList();
            return response.data;
        }
    }
}