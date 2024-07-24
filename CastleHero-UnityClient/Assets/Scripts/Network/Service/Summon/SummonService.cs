using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;

namespace RGLabs.Network.Service.Summon
{
    public class SummonService
    {
        public async UniTask<SummonResult> SummonOnce(int eventId, int coastId) => (await Summon(eventId, coastId, 1));

        public async UniTask<SummonResult> SummonTenth(int eventId, int coastId) => await Summon(eventId, coastId, 10);

        private async UniTask<SummonResult> Summon(int eventId, int coastId, int count)
        {
            var eventChartId = Storage.db.summons.Id;
            var listChartId = Storage.db.summonGroups.Id;
            var response = await BackendWrapper.Summon(eventId, coastId, count, eventChartId, listChartId);

            return response.data;
        }
    }
}