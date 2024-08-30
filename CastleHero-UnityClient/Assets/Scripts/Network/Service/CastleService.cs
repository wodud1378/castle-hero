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
                return Result<CastleGrowth>.Error(Error.DBReadFailed);

            var record = read.data.gameRecord;
            var currency = read.data.currency;
            
            ProcessLevelUp(record, currency, out var lv, out var error);
            if (error != Error.None)
                return Result<CastleGrowth>.Error(error);
            
            var write = await UpdateTables(new Dictionary<Table, object>
            {
                { Table.GameRecord, record },
                { Table.Currency, currency }
            });

            return write.IsSuccess
                ? Result<CastleGrowth>.Complete(new() { lv = lv, })
                : Result<CastleGrowth>.Error(write.error);
        }

        private void ProcessLevelUp(GameRecordDto record, CurrencyDto currency, out int lv, out Error error)
        {
            var db = Storage.db.castles;
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