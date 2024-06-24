using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using UnityEngine;
using Random = System.Random;

namespace RGLabs.Network.Service.Summon
{
    public class LocalSummonService : ISummonService
    {
        public SummonEntity Entity { get; }

        private readonly SummonGroupEntity[] _groupEntities;
        private readonly Random _random = new();

        public LocalSummonService(int summonEventId)
        {
            Entity = Storage.db.summons.TryFind(summonEventId, out var entity)
                ? entity
                : default;

            _groupEntities = Storage.db.summonGroups.Map(Entity.groupId);
        }
        
        public UniTask<ISummonResult> SummonOnce()
        {
            int id = GetRandomId();
            
            return UniTask.FromResult(GetSummonResult(id)[0]); 
        }

        public UniTask<ISummonResult[]> SummonTenth()
        {
            var array = new int[10];
            for (int i = 0; i < 10; ++i)
            {
                array[i] = GetRandomId();
            }

            return UniTask.FromResult(GetSummonResult(array)); 
        }

        private ISummonResult[] GetSummonResult(params int[] ids)
        {
            int length = ids.Length;
            var owned = Storage.userRepository.characters.ToArray();
            var result = new ISummonResult[length];
            for (int i = 0; i < length; i++)
            {
                int id = ids[i];
                var groupEntity = _groupEntities.FirstOrDefault(x => x.Id == id);
                if (!groupEntity.IsValid)
                    continue;

                int unitId = groupEntity.unitId;
                var exist = owned.FirstOrDefault(x => x.id == unitId);
                if (exist != default)
                {
                    result[i] = new SummonedSoul
                    {
                        Id = groupEntity.soulId,
                        quantity = groupEntity.soulCount
                    };
                }
                else
                {
                    result[i] = new SummonedUnit
                    {
                        Id = unitId
                    };
                }
            }

            return result;
        }
        
        private int GetRandomId()
        {
            float totalWeight = _groupEntities.Sum(x => x.weight);
            float random = (float)(_random.NextDouble() * totalWeight);

            int length = _groupEntities.Length;
            float sum = 0f;
            int index = 0;
            while (sum <= random && index++ < length)
            {
                sum += _groupEntities[index].weight;
            }

            index = Mathf.Clamp(index, 0, length - 1);
            return _groupEntities[index].Id;
        }
    }
}