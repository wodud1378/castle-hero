using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;

namespace CastleHero.Network.Impl.Local.Services
{
    public class LocalSummonService : LocalNetworkServiceBase, ISummonService
    {
        public LocalSummonService(IServiceLocator sl, LocalUserDataStore store) : base(sl, store) { }

        public UniTask<Result<Summon>> SummonOnce(int eventId, int costIndex) => Summon(eventId, costIndex, 1);

        public UniTask<Result<Summon>> SummonTenth(int eventId, int costIndex) => Summon(eventId, costIndex, 10);

        private UniTask<Result<Summon>> Summon(int eventId, int costIndex, int count)
        {
            if (!Db.Summons.TryFind(eventId, out var entity))
                return UniTask.FromResult(Result<Summon>.Error(Error.DataNotFound));

            if (!costIndex.IsValidIndex(entity.costItems, entity.valuePerOnce, entity.valuePerTenth))
                return UniTask.FromResult(Result<Summon>.Error(Error.InvalidRequest));

            var groupEntities = Db.SummonGroups.Map(entity.groupId);
            if (groupEntities == null || groupEntities.Length == 0)
                return UniTask.FromResult(Result<Summon>.Error(Error.InvalidRequest));

            var costId = entity.costItems[costIndex];
            var costValue = count switch
            {
                1 => entity.valuePerOnce[costIndex],
                10 => entity.valuePerTenth[costIndex],
                _ => -1
            };

            if (costValue == -1)
                return UniTask.FromResult(Result<Summon>.Error(Error.InvalidRequest));

            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<Summon>.Error(get.error));

            var userData = get.data;
            switch (costId)
            {
                case Constants.PaidDiaId:
                case Constants.FreeDiaId:
                    if (!userData.currency.TryConsumeDia(costValue))
                        return UniTask.FromResult(Result<Summon>.Error(Error.NotEnoughCurrency));

                    break;
                case Constants.GoldId:
                    if (userData.currency.gold < costValue)
                        return UniTask.FromResult(Result<Summon>.Error(Error.NotEnoughCurrency));

                    userData.currency.gold -= costValue;
                    break;
                default:
                    if (!userData.inventory.items.TryConsumeItem(costId, costValue))
                        return UniTask.FromResult(Result<Summon>.Error(Error.NotEnoughItem));

                    break;
            }

            var units = userData.characters.units;
            var unitIds = GetSummonResult(groupEntities, count);
            var souls = new List<IItem>();

            UnitGen.AddUnits(unitIds, units, souls, false,
                out var newUnitIndex, out var itemAdded);

            var soulResult = new List<ISummoned>();
            if (itemAdded)
            {
                userData.inventory.items.Join(souls);
                foreach (var soul in souls)
                {
                    soulResult.Add(new SummonedSoul
                    {
                        Id = soul.ItemId,
                        quantity = soul.Quantity
                    });
                }
            }

            var summonResult = new List<ISummoned>();
            if (newUnitIndex != -1)
            {
                for (int i = newUnitIndex; i < units.Count; ++i)
                {
                    var unit = units[i];
                    summonResult.Add(new SummonedUnit
                    {
                        Id = unit.id,
                        lv = unit.lv,
                        rate = unit.rate,
                    });
                }
            }

            summonResult.AddRange(soulResult);

            Save(userData);

            return UniTask.FromResult(Result<Summon>.Complete(new Summon { list = summonResult }));
        }

        private List<int> GetSummonResult(SummonGroupEntity[] entities, int count)
        {
            var result = new List<int>();
            var map = entities.Select(x => (x.unitId, x.weight)).ToList();
            var totalWeight = map.Sum(x => x.weight);

            for (int i = 0; i < count; ++i)
            {
                float sum = 0f;
                float rand = Random.value * totalWeight;
                using var itr = map.GetEnumerator();
                var current = map[0].unitId;
                while (sum <= rand && itr.MoveNext())
                {
                    current = itr.Current.unitId;
                    sum += itr.Current.weight;
                }

                result.Add(current);
            }

            return result;
        }
    }
}
