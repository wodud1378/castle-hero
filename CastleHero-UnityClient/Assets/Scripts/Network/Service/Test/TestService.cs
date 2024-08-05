using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Test
{
    public class TestService : NetworkServiceBase
    {
        public async UniTask<Currency> AddCurrency(Currency currency)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(currency.paidDia), currency.paidDia),
                new(nameof(currency.freeDia), currency.freeDia),
                new(nameof(currency.gold), currency.gold)
            };

            var response = await InvokeFunc("AddCurrency", parameters, ConvertFunctionResponse<Currency>());

            return response.data;
        }

        public async UniTask<Inventory> AddItems(int[] ids, int[] quantities)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(ids), ids),
                new(nameof(quantities), quantities),
            };
            
            var response = await InvokeFunc("AddItems", parameters, ConvertFunctionResponse<Inventory>());

            return response.data;
        }
    }
}