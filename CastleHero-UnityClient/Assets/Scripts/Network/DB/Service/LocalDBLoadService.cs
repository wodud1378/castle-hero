using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Localize;
using RGLabs.Data.DB;
using RGLabs.Data.Load;
using RGLabs.Network.Service.Boot;

namespace RGLabs.Network.DB.Service
{
    public class LocalDBLoadService : IDBLoadService
    {
        private readonly CsvToDatabase _loader = new();
        
        public async UniTask<(DBCollections db, LocalizeText localize)> Load(ChartInfo[] _)
        {
            DBCollections collections = new();
            
            var tasks = new List<UniTask>
            {
                _loader.Load<StageDB>(x => collections.stages = x),
                _loader.Load<WaveDB>(x => collections.waves = x),
                _loader.Load<UnitDB>(x => collections.units = x),
                _loader.Load<UnitLevelDB>(x => collections.levels = x),
                _loader.Load<UnitRateDB>(x => collections.rates = x),
                _loader.Load<UnitBalanceDB>(x => collections.balances = x),
                _loader.Load<SkillDB>(x => collections.skills = x, true),
                _loader.Load<CastleDB>(x => collections.castles = x),
                _loader.Load<SummonDB>(x => collections.summons = x),
                _loader.Load<SummonGroupDB>(x => collections.summonGroups = x), 
                _loader.Load<ItemDB>(x => collections.items = x),
                _loader.Load<ShopDB>(x => collections.shop = x),
                _loader.Load<ShopItemGroupDB>(x => collections.shopGroup = x),
            };

            await UniTask.WhenAll(tasks);

            collections.units.CacheUnitSizes();

            return (collections, null);
        }
    }
}