using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using LitJson;
using CastleHero.Common.Secure;
using CastleHero.Data.DB;
using CastleHero.Data.Load;
using CastleHero.Network.DB;
using CastleHero.Network.DB.Service;
using CastleHero.Network.Impl.Backend.Boot;
using CastleHero.Network.Service.Boot;
using UnityEngine;

namespace CastleHero.Network.Impl.Backend.DB
{
    public class BackendDBLoadService : IDBLoadService
    {
        private readonly JsonToDatabase _converter = new();
        private readonly BackendInitService _service;

        public BackendDBLoadService(BackendInitService service) => _service = service;

        public async UniTask<DBCollections> InitialLoad(ChartInfo[] chartList)
        {
            DBCollections collections = new();

            var tasks = new List<UniTask<(string chartName, int id, JsonData rawData)>>();
            foreach (var chartInfo in chartList)
            {
                var name = chartInfo.chartName;
                var id = chartInfo.selectedChartFileId;
                tasks.Add(GetChartContent(name, id));
            }

            var results = await UniTask.WhenAll(tasks);
            var map = results
                .ToDictionary(x => x.chartName, y => (y.id, y.rawData));

            LoadInstance<StageDB>(map, x => collections.Stages = x);
            LoadInstance<WaveDB>(map, x => collections.Waves = x);
            LoadInstance<CastleDB>(map, x => collections.Castles = x);
            LoadInstance<UnitDB>(map, x => collections.Units = x);
            LoadInstance<ElementDB>(map, x => collections.Elements = x);
            LoadInstance<UnitLevelDB>(map, x => collections.Levels = x);
            LoadInstance<UnitRateDB>(map, x => collections.Rates = x);
            LoadInstance<UnitBalanceDB>(map, x => collections.Balances = x);
            LoadInstance<SkillDB>(map, x => collections.Skills = x);
            LoadInstance<SummonDB>(map, x => collections.Summons = x);
            LoadInstance<SummonGroupDB>(map, x => collections.SummonGroups = x);
            LoadInstance<ItemDB>(map, x => collections.Items = x);
            LoadInstance<EquipItemStatDB>(map, x => collections.EquipmentStats = x);
            LoadInstance<ShopDB>(map, x => collections.Shop = x);
            LoadInstance<ShopItemGroupDB>(map, x => collections.ShopGroup = x);
            LoadInstance<DungeonDB>(map, x => collections.Dungeons = x);
            LoadInstance<DungeonRewardDB>(map, x => collections.DungeonRewards = x);

            collections.Units.CacheUnitSizes();

            return collections;
        }

#if UNITY_EDITOR
        private async UniTask<(string chartName, int id, JsonData rawData)> GetChartContent(string chartName, int id)
        {
            var response = await _service.GetChartContent(id.ToString());
            var raw = response.raw.FlattenRows();

            return (chartName, id, raw);
        }
#else
        private async UniTask<(string chartName, int id, JsonData rawData)> GetChartContent(string chartName, int id)
        {
            var versionKey = $"{chartName}_version";
            var lastVersion = PlayerPrefs.GetInt(versionKey, -1);
            if (lastVersion == -1 || lastVersion != id)
            {
                var result = await _service.GetChartContent(id.ToString());
                var raw = result.raw.FlattenRows();

                PlayerPrefs.SetInt(versionKey, id);
                EncryptStore.SetString(chartName, raw.ToJson());
                return (chartName, id, raw);
            }

            var text = EncryptStore.GetString(chartName);
            return (chartName, id, JsonMapper.ToObject(text));
        }
#endif

        private void LoadInstance<T>(Dictionary<string, (int id, JsonData json)> map, Action<T> onResult)
            where T : class, IDataBase
        {
            var type = typeof(T);
            var att = type.GetCustomAttribute<DBAttribute>();
            if (att == null)
                return;

            if (!map.TryGetValue(att.ChartName, out var data))
                return;

            var obj = _converter.Convert<T>(data.json, type == typeof(SkillDB) || type == typeof(ItemDB));
            obj.Id = data.id;
            onResult.Invoke(obj);
        }
    }
}
