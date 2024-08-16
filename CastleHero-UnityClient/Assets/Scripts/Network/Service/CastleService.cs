using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service
{
    public class CastleService : NetworkServiceBase
    {
        public async UniTask<CastleGrowth> LevelUp()
        {
            var response = await InvokeFunc("CastleLevelUp", null, ConvertFunctionResponse<CastleGrowth>());

            return response.data;
        }
    }
}