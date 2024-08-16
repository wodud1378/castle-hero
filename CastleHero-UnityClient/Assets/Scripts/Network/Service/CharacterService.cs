using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service
{
    public class CharacterService : NetworkServiceBase
    {
        private const string LevelUpMethod = "LevelUp";
        private const string UpgradeMethod = "Upgrade";
        
        public async UniTask<GrowthResult> LevelUp(int unitId, int itemId, int itemQty) 
            => await CallGrowth(LevelUpMethod, unitId, itemId, itemQty);

        public async UniTask<GrowthResult> Upgrade(int unitId, int itemId, int itemQty) 
            => await CallGrowth(UpgradeMethod, unitId, itemId, itemQty);

        private async UniTask<GrowthResult> CallGrowth(string method, int unitId, int itemId, int itemQty)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(unitId), unitId),
                new(nameof(itemId), itemId),
                new(nameof(itemQty), itemQty),
            };
            
            var response = await InvokeFunc(method, parameters, ConvertFunctionResponse<GrowthResult>());
            return response.data;
        }
    }
}