using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Network.Service
{
    public class SummonService : NetworkServiceBase
    {
        public UniTask<Result<Summon>> SummonOnce(int eventId, int costIndex) => Summon(eventId, costIndex, 1);

        public UniTask<Result<Summon>> SummonTenth(int eventId, int costIndex) => Summon(eventId, costIndex, 10);

        private async UniTask<Result<Summon>> Summon(int eventId, int costIndex, int count)
        {
            if (!Storage.db.summons.TryFind(eventId, out var entity))
                return Result<Summon>.Error(Error.DataNotFound);

            if (!costIndex.IsValidIndex(entity.costItems, entity.valuePerOnce, entity.valuePerTenth))
                return Result<Summon>.Error(Error.InvalidRequest);

            var groupEntities = Storage.db.summonGroups.Map(entity.groupId);
            if(groupEntities == null || groupEntities.Length == 0)
                return Result<Summon>.Error(Error.InvalidRequest);

            var costId = entity.costItems[costIndex];
            var costValue = count switch
            {
                1 => entity.valuePerOnce[costIndex],
                10 => entity.valuePerTenth[costIndex],
                _ => -1
            };

            if (costValue == -1)
                return Result<Summon>.Error(Error.InvalidRequest);

            var get = await GetTables(Table.Currency, Table.Inventory, Table.Character);
            if (!get.IsSuccess)
                return Result<Summon>.Error(get.error);

            var userData = get.data;
            bool updateCurrency = false;
            bool updateInventory = false;
            switch (costId)
            {
                case Constants.PaidDiaId:
                case Constants.FreeDiaId:
                    if(!userData.currency.TryConsumeDia(costValue))
                        return Result<Summon>.Error(Error.NotEnoughCurrency);

                    updateCurrency = true;
                    break;
                case Constants.GoldId:
                    if(userData.currency.gold < costValue)
                        return Result<Summon>.Error(Error.NotEnoughCurrency);

                    userData.currency.gold -= costValue;
                    updateCurrency = true;
                    break;
                default:
                    if(!userData.inventory.items.TryConsumeItem(costId, costValue))
                        return Result<Summon>.Error(Error.NotEnoughItem);

                    updateInventory = true;
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
            
            updateInventory = updateInventory || itemAdded;

            var tables = new Dictionary<Table, object>();
            if(updateCurrency)
                tables.Add(Table.Currency, userData.currency);
            
            if(updateInventory)
                tables.Add(Table.Inventory, userData.inventory);

            var summonResult = new List<ISummoned>();
            if (newUnitIndex != -1)
            {
                tables.Add(Table.Character, userData.characters);
                for(int i = newUnitIndex; i < units.Count; ++i)
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

            var update = await UpdateTables(tables);
            return update.IsSuccess 
                ? Result<Summon>.Complete(new Summon { list = summonResult })
                : Result<Summon>.Error(update.error); 
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