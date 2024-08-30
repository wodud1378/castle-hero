using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Test
{
    public class TestService : NetworkServiceBase
    {
        public async UniTask<Result> AddCurrency(CurrencyDto add)
        {
            var get = await GetTable<CurrencyDto>(Table.Currency);
            if (!get.IsSuccess)
                return Result.Error(get.error);

            var currency = get.data;
            currency += add;
            
            var update = await UpdateTable(Table.Currency, currency);
            if (!update.IsSuccess)
                return Result.Error(update.error);
            
            Storage.userRepository.currency.Update(currency);
            return Result.Complete();
        }
        
        public async UniTask<Result> AddItems(int[] ids, int[] quantities)
        {
            var get = await GetTables(Table.Currency, Table.Inventory);
            if (!get.IsSuccess)
                return Result.Error(get.error);

            var userData = get.data;
            ItemGen.NewItems(ids, quantities, userData.currency, userData.inventory.items);

            var update = await UpdateTables(userData);
            return update.IsSuccess
                ? Result.Complete()
                : Result.Error(update.error);
        }
    }
}