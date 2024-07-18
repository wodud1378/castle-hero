using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Character
{
    public class CharacterService
    {
        private const string LevelUpMethod = "LevelUp";
        private const string UpgradeMethod = "Upgrade";
        
        public async UniTask<GrowthResult> LevelUp(int unitId, int itemId, int itemQty) 
            => await CallGrowth(LevelUpMethod, Storage.db.levels.Id, unitId, itemId, itemQty);

        public async UniTask<GrowthResult> Upgrade(int unitId, int itemId, int itemQty) 
            => await CallGrowth(UpgradeMethod, Storage.db.rates.Id, unitId, itemId, itemQty);

        private async UniTask<GrowthResult> CallGrowth(string method, int chartId, int unitId, int itemId, int itemQty) 
            => (await BackendWrapper.Growth(method, chartId, unitId, itemId, itemQty)).data;
    }
}