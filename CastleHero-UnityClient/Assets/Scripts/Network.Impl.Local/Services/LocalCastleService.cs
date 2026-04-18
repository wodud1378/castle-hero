using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Impl.Local.Services
{
    public class LocalCastleService : LocalNetworkServiceBase, ICastleService
    {
        public LocalCastleService(IServiceLocator sl, LocalUserDataStore store) : base(sl, store) { }

        public UniTask<Result<CastleGrowth>> LvUp()
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<CastleGrowth>.Error(Error.DBReadFailed));

            var userData = get.data;
            var record = userData.gameRecord;
            var currency = userData.currency;

            ProcessLevelUp(record, currency, out var lv, out var error);
            if (error != Error.None)
                return UniTask.FromResult(Result<CastleGrowth>.Error(error));

            Save(userData);

            return UniTask.FromResult(Result<CastleGrowth>.Complete(new CastleGrowth { lv = lv }));
        }

        private void ProcessLevelUp(GameRecordDto record, CurrencyDto currency, out int lv, out Error error)
        {
            var db = Db.Castles;
            lv = record.castleLv;
            if (db.MaxLv <= lv)
            {
                error = Error.AlreadyMaxLv;
                return;
            }

            if (!db.TryFind(lv, out var entity))
            {
                error = Error.DataNotFound;
                return;
            }

            if (currency.gold < entity.lvUpPrice)
            {
                error = Error.NotEnoughCurrency;
                return;
            }

            ++lv;
            currency.gold -= entity.lvUpPrice;
            record.castleLv = lv;
            error = Error.None;
        }
    }
}
