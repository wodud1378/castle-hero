using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Stage
{
    public class StageService : NetworkServiceBase
    {
        public async UniTask<bool> StartGame(int stage)
        {
            var parameter = new List<KeyValuePair<string, object>> { new(nameof(stage), stage) };
            var response = await InvokeFunc("StartGame", parameter, ConvertFunctionResponse<bool>());

            return response.data;
        }
        
        public async UniTask<StageCleared> SetClear(int stage)
        {
            var parameters = new List<KeyValuePair<string, object>> { new(nameof(stage), stage), };
            var response = await InvokeFunc("StageClear", parameters, ConvertFunctionResponse<StageCleared>());

            return response.data;
        }
    }
}