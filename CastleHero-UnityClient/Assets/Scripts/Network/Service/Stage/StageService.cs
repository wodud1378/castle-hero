using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Stage
{
    public class StageService
    {
        public async UniTask<StageCleared> SetClear(int stage)
        {
            var chartIds = new int[]
            {
                Storage.db.stages.Id,
                Storage.db.levels.Id,
                Storage.db.items.Id,
                Storage.db.stats.Id
            };

            var response = await BackendWrapper.SetStageClear(
                chartIds[0], chartIds[1], chartIds[2], chartIds[3],
                stage);

            return response.data;
        }
    }
}