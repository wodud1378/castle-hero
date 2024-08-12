using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Castle
{
    public class CastleService : NetworkServiceBase
    {
        public async UniTask<CastleGrowth> LevelUp()
        {
            var response = await InvokeFunc("CastleLevelUp", 
                new(), ConvertFunctionResponse<CastleGrowth>());

            return response.data;
        }
    }
}