using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Test
{
    public class TestService
    {
        public async UniTask<Currency> AddCurrency(Currency currency) 
            => (await BackendWrapper.TEST_AddCurrency(currency.paidDia, currency.freeDia, currency.gold)).data;
        
        public async UniTask<Inventory> AddItems(int[] ids, int[] quantities) 
            => (await BackendWrapper.TEST_AddItems(ids, quantities)).data;
    }
}