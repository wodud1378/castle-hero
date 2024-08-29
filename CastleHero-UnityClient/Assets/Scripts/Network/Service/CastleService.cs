using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service
{
    public class CastleService : NetworkServiceBase
    {
        public async UniTask<Result<CastleGrowth>> LvUp()
        {
            var read = await GetTables(Table.GameRecord, Table.Currency);
            if(read.error != Error.None)
                return Result<CastleGrowth>.FromError(Error.DBReadFailed);

            var record = read.data.gameRecord;
            var currency = read.data.currency;
            
            ProcessLevelUp(record, currency, out var lv, out var error);
            if (error != Error.None)
                return Result<CastleGrowth>.FromError(error);
            
            var write = await UpdateTables(new Dictionary<Table, object>
            {
                { Table.GameRecord, record },
                { Table.Currency, currency }
            });

            return write.IsSuccess
                ? Result<CastleGrowth>.From(new() { lv = lv, })
                : Result<CastleGrowth>.FromError(write.error);
        }

        private void ProcessLevelUp(GameRecordDto record, CurrencyDto currency, out int lv, out Error error)
        {
            var chart = Storage.db.castles;
            lv = record.castleLv;
            int maxLv = chart[^1].Id;
            if (maxLv <= lv)
            {
                error = Error.AlreadyMaxLv;
                return;
            }

            if (!chart.TryFind(lv, out var entity))
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