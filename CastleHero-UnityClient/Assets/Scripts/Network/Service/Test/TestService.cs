using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Test
{
    public class TestService : NetworkServiceBase
    {
        public async UniTask<CurrencyDto> AddCurrency(CurrencyDto currency)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(currency.paidDia), currency.paidDia),
                new(nameof(currency.freeDia), currency.freeDia),
                new(nameof(currency.gold), currency.gold)
            };

            var response = await InvokeFunc("AddCurrency", parameters, ConvertFunctionResponse<CurrencyDto>());

            return response.data;
        }

        public async UniTask<InventoryDto> AddItems(int[] ids, int[] quantities)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(ids), ids),
                new(nameof(quantities), quantities),
            };
            
            var response = await InvokeFunc("AddItems", parameters, ConvertFunctionResponse<InventoryDto>());

            return response.data;
        }
    }
}