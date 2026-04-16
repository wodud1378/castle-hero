using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Data.DB;
using CastleHero.Data.Load;
using CastleHero.Network.DB;
using CastleHero.Network.DB.Service;
using CastleHero.Network.Service.Boot;

namespace CastleHero.Network.Impl.Local
{
    public class LocalDBLoadService : IDBLoadService
    {
        private readonly CsvToDatabase _loader = new();

        public async UniTask<DBCollections> InitialLoad(ChartInfo[] _)
        {
            DBCollections collections = new();

            var tasks = new List<UniTask>
            {
                _loader.Load<StageDB>(x => collections.Stages = x),
                _loader.Load<WaveDB>(x => collections.Waves = x),
                _loader.Load<UnitDB>(x => collections.Units = x),
                _loader.Load<UnitLevelDB>(x => collections.Levels = x),
                _loader.Load<UnitRateDB>(x => collections.Rates = x),
                _loader.Load<UnitBalanceDB>(x => collections.Balances = x),
                _loader.Load<SkillDB>(x => collections.Skills = x, true),
                _loader.Load<CastleDB>(x => collections.Castles = x),
                _loader.Load<SummonDB>(x => collections.Summons = x),
                _loader.Load<SummonGroupDB>(x => collections.SummonGroups = x),
                _loader.Load<ItemDB>(x => collections.Items = x),
                _loader.Load<ShopDB>(x => collections.Shop = x),
                _loader.Load<ShopItemGroupDB>(x => collections.ShopGroup = x),
            };

            await UniTask.WhenAll(tasks);

            collections.Units.CacheUnitSizes();

            return collections;
        }
    }
}
