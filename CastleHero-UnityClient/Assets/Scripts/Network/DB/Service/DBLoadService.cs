using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using LitJson;
using RGLabs.Common.Localize;
using RGLabs.Common.Secure;
using RGLabs.Data.DB;
using RGLabs.Data.Load;
using RGLabs.Network.Service.Boot;
using UnityEngine;

namespace RGLabs.Network.DB.Service
{
    public class DBLoadService : IDBLoadService
    {
        private readonly JsonToDatabase _converter = new();
        private readonly InitService _service;

        public DBLoadService(InitService service) => _service = service;

        public async UniTask<(DBCollections db, LocalizeText localize)> InitialLoad(ChartInfo[] chartList)
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
            LoadInstance<ShopDB>(map, x => collections.shop = x);
            LoadInstance<ShopItemGroupDB>(map, x => collections.shopGroup = x);
            LoadInstance<DungeonDB>(map, x => collections.dungeons = x);
            LoadInstance<DungeonRewardDB>(map, x => collections.dungeonRewards = x);

            var localize = map.TryGetValue("localize", out var data)
                ? new LocalizeText(data.rawData)
                : null;

            collections.units.CacheUnitSizes();

            return (collections, localize);
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
                var response = await _service.GetChartContent(id.ToString());
                var raw = response.rawData;
                
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