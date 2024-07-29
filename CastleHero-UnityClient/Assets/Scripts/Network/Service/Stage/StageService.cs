using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Stage
{
    public class StageService
    {
        public async UniTask<StageCleared> SetClear(int stage)
        {
            var response = await BackendWrapper.SetStageClear(stage);

            return response.data;
        }
    }
}