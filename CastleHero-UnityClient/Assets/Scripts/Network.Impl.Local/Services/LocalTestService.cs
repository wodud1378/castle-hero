using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Impl.Local.Services
{
    public class LocalTestService : LocalNetworkServiceBase, ITestService
    {
        public LocalTestService(IServiceLocator sl, LocalUserDataStore store) : base(sl, store) { }

        public UniTask<Result> AddItems(int[] ids, int[] quantities)
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result.Error(get.error));

            var userData = get.data;
            ItemGen.NewItems(ids, quantities, userData.currency, userData.inventory.items);

            Save(userData);

            return UniTask.FromResult(Result.Complete());
        }

        public UniTask<Result> AddCurrency(CurrencyDto add)
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result.Error(get.error));

            var userData = get.data;
            userData.currency += add;

            Save(userData);

            return UniTask.FromResult(Result.Complete());
        }
    }
}
