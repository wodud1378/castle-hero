using Cysharp.Threading.Tasks;
using CastleHero.Data;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;

using CastleHero.Common.Pattern;
using CastleHero.Data.Repositories;
namespace CastleHero.Network.Impl.Backend.Services
{
    public class BackendTestService : BackendNetworkServiceBase, ITestService
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

            ServiceLocator.Get<IUserRepository>().Currency.Update(currency);
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
