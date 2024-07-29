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
            => await CallGrowth(LevelUpMethod, unitId, itemId, itemQty);

        public async UniTask<GrowthResult> Upgrade(int unitId, int itemId, int itemQty) 
            => await CallGrowth(UpgradeMethod, unitId, itemId, itemQty);

        private async UniTask<GrowthResult> CallGrowth(string method, int unitId, int itemId, int itemQty) 
            => (await BackendWrapper.Growth(method, unitId, itemId, itemQty)).data;
    }
}