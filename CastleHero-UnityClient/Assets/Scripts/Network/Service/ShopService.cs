using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service
{
    public class ShopService : NetworkServiceBase
    {
        public async UniTask<ItemBought> Buy(int type, int id, int quantity)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(type), type),
                new(nameof(id), id),
                new(nameof(quantity), quantity),
            };

            var response = await InvokeFunc("Buy", parameters, ConvertFunctionResponse<ItemBought>());
            return response.data;
        }
    }
}