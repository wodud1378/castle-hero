using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service
{
    public class SummonService : NetworkServiceBase
    {
        public async UniTask<SummonResult> SummonOnce(int eventId, int coastId) => (await Summon(eventId, coastId, 1));

        public async UniTask<SummonResult> SummonTenth(int eventId, int coastId) => await Summon(eventId, coastId, 10);

        private async UniTask<SummonResult> Summon(int eventId, int coastId, int count)
        {
            var parameters = new List<KeyValuePair<string, object>>()
            {
                new(nameof(eventId), eventId),
                new(nameof(coastId), coastId),
            };

            
            var response = await InvokeFunc($"SummonX{count}", parameters, ConvertFunctionResponse<SummonResult>());

            return response.data;
        }
    }
}