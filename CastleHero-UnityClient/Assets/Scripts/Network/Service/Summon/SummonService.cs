using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;

namespace RGLabs.Network.Service.Summon
{
    public class SummonService : ISummonService
    {
        public SummonEntity Entity { get; }

        public SummonService(int summonEventId)
        {
            Entity = Storage.db.summons.TryFind(summonEventId, out var entity)
                ? entity
                : default;
        }
        
        public async UniTask<ISummonResult> SummonOnce() => (await Summon(1)).FirstOrDefault();

        public async UniTask<List<ISummonResult>> SummonTenth() => await Summon(10);

        private async UniTask<List<ISummonResult>> Summon(int count)
        {
            if (!Storage.db.summons.TryFindIndex(Entity.Id, out int index))
                return default;

            var eventChartId = Entity.Id;
            var listChartId = Storage.db.summonGroups.Id;
            var response = await BackendWrapper.Summon(count, index, eventChartId, listChartId);

            return response.data;
        }
    }
}