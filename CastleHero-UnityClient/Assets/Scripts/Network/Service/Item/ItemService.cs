using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Item
{
    public class ItemService: NetworkServiceBase
    {
        public async UniTask<OpenBoxResult> OpenBox(int boxItemId, int itemQty)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(boxItemId), boxItemId),
                new(nameof(itemQty), itemQty),
            };
            
            var response = await InvokeFunc("OpenBox", parameters, ConvertFunctionResponse<OpenBoxResult>());
            return response.data;
        }

        public async UniTask<ItemsSold> Sell(int[] ids, int[] quantities)
        {
            var parameters = new List<KeyValuePair<string, object>>()
            {
                new(nameof(ids), ids),
                new(nameof(quantities), quantities)
            };

            var response = await InvokeFunc("Sell", parameters, ConvertFunctionResponse<ItemsSold>());
            return response.data;
        }

        public async UniTask<ItemBought> Buy(int id, int quantity)
        {
            var parameters = new List<KeyValuePair<string, object>>()
            {
                new(nameof(id), id),
                new(nameof(quantity), quantity)
            };

            var response = await InvokeFunc("Sell", parameters, ConvertFunctionResponse<ItemBought>());
            return response.data;
        }
    }
}