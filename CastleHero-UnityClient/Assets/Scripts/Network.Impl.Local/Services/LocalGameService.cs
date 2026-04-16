using System;
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
using Random = UnityEngine.Random;

namespace CastleHero.Network.Impl.Local.Services
{
    public class LocalGameService : LocalNetworkServiceBase, IGameService
    {
        public LocalGameService(LocalUserDataStore store) : base(store) { }

        public UniTask<Result<List<int>>> GetOpenedDungeonLayers()
        {
            var map = new Dictionary<int, List<DayOfWeek>>();
            ServiceLocator.Get<IDBProvider>().Dungeons.BinarySearch(x =>
            {
                if (map.ContainsKey(x.Layer))
                    return;

                map.Add(x.Layer, x.OpenDaysOfWeek());
            });

#if UNITY_EDITOR
            if (NetworkConfig.Current != null && NetworkConfig.Current.openAllDungeons)
                return UniTask.FromResult(Result<List<int>>.Complete(map.Keys.ToList()));
#endif
            var dow = ServerTime.Now.DayOfWeek;
            var opened = map.Where(x => x.Value.Contains(dow)).Select(x => x.Key).ToList();
            return UniTask.FromResult(Result<List<int>>.Complete(opened));
        }

        public UniTask<Result> Start(GameType type, int id)
        {
            var error = CanEntrance(type, id);
            return UniTask.FromResult(error != Error.None ? Result.Error(error) : Result.Complete());
        }

        private Error CanEntrance(GameType type, int id)
        {
            if (!ServiceLocator.Get<IDBProvider>().TryLoadGameEntity(type, id, out var entity))
                return Error.DataNotFound;

            var get = Get();
            if (!get.IsSuccess)
                return get.error;

            var userData = get.data;
            if (!HasEnoughAp(userData.stamina, entity.Ap))
                return Error.NotEnoughAp;

            Save(userData);

            return CheckIsOpened(userData.gameRecord, entity);
        }

        private Error CheckIsOpened(GameRecordDto record, IGameEntity entity)
        {
            switch (entity.Type)
            {
                case GameType.Stage:
                    return GetLatestStageLv(record) + 1 < entity.Lv ? Error.NotOpened : Error.None;
                case GameType.Dungeon:
                    if (entity is not DungeonEntity dungeonEntity)
                        return Error.InvalidRequest;

                    var dow = ServerTime.Now.DayOfWeek;
                    if (!dungeonEntity.OpenDaysOfWeek().Contains(dow))
                        return Error.NotOpened;

                    var dungeonRecord = GetLatestDungeonRecord(record, dungeonEntity);
                    int lv = dungeonRecord?.lastClearedLv ?? 0;
                    return lv + 1 < entity.Lv ? Error.NotOpened : Error.None;
            }

            return Error.Unknown;
        }

        public UniTask<Result<GameCleared>> Clear(GameType type, int id)
        {
            if (!ServiceLocator.Get<IDBProvider>().TryLoadGameEntity(type, id, out var entity))
                return UniTask.FromResult(Result<GameCleared>.Error(Error.DataNotFound));

            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<GameCleared>.Error(get.error));

            var userData = get.data;
            RecoverStamina(userData.stamina);

            if (userData.stamina.point < entity.Ap)
                return UniTask.FromResult(Result<GameCleared>.Error(Error.NotEnoughAp));

            var openError = CheckIsOpened(userData.gameRecord, entity);
            if (openError != Error.None)
                return UniTask.FromResult(Result<GameCleared>.Error(openError));

            userData.stamina.point -= entity.Ap;

            var result = new GameCleared
            {
                id = entity.Id,
                exp = entity.Exp,
                isFirstClear = IsFirstClear(userData.gameRecord, entity),
                transitions = GetUnitTransition(
                    userData.formation, userData.characters, entity.Exp,
                    out _)
            };

            (CurrencyDto currency, List<IItem> items) reward;
            switch (entity)
            {
                case StageEntity stage:
                    reward = GetStageRewards(stage, result.isFirstClear);
                    break;
                case DungeonEntity dungeon:
                    reward = GetDungeonRewards(dungeon);
                    break;
                default:
                    return UniTask.FromResult(Result<GameCleared>.Error(Error.Unknown));
            }

            result.currency = reward.currency;
            result.items = reward.items;

            UpdateRecord(userData.gameRecord, entity);

            if (!reward.currency.IsEmpty())
                userData.currency += reward.currency;

            if (reward.items.Count > 0)
                userData.inventory.items.Join(reward.items);

            Save(userData);

            return UniTask.FromResult(Result<GameCleared>.Complete(result));
        }

        private (CurrencyDto currency, List<IItem> items) GetStageRewards(StageEntity entity, bool isFirstClear)
        {
            var currency = new CurrencyDto();
            var items = new List<IItem>();
            if (isFirstClear)
            {
                ItemGen.NewItems(
                    entity.firstClearRewardId,
                    entity.firstClearRewardQty,
                    currency, items);
            }

            currency.gold += Random.Range(entity.MinGold, entity.MaxGold + 1);

            if (entity.propItemId != 0 &&
                entity.propItemQty > 0 &&
                Random.value <= entity.itemPer)
            {
                ItemGen.NewItems(
                    entity.propItemId,
                    entity.propItemQty,
                    currency, items);
            }

            return (currency, items);
        }

        private (CurrencyDto currency, List<IItem> items) GetDungeonRewards(DungeonEntity entity)
        {
            var currency = new CurrencyDto();
            var items = new List<IItem>();
            if (ServiceLocator.Get<IDBProvider>().DungeonRewards.TryFind(entity.rewardGroup, out var rewardEntity))
            {
                var selected = rewardEntity.itemIds.Select((id, index) => new
                {
                    id,
                    min = rewardEntity.minQuantities[index],
                    max = rewardEntity.maxQuantities[index],
                    prob = rewardEntity.probabilities[index]
                }).Where(item => item.prob >= Random.value).Select(item => new
                {
                    item.id,
                    quantity = Random.Range(item.min, item.max + 1)
                }).ToArray();

                var ids = selected.Select(x => x.id).ToArray();
                var quantities = selected.Select(x => x.quantity).ToArray();
                ItemGen.NewItems(ids, quantities, currency, items);
            }

            currency.gold += Random.Range(entity.MinGold, entity.MaxGold + 1);

            return (currency, items);
        }

        private List<UnitTransition> GetUnitTransition(FormationDto formation, CharactersDto characters, int addExp,
            out bool updated)
        {
            updated = false;

            var transitions = new List<UnitTransition>();
            var units = characters.units
                .Where(unit => formation.fieldUnits.FindIndex(x => x.id == unit.id) != -1)
                .ToList();

            if (addExp == 0 || units.Count == 0)
            {
                foreach (var unit in units)
                {
                    transitions.Add(UnitTransition.Create(unit));
                }
            }
            else
            {
                int perUnit = addExp / units.Count;
                foreach (var unit in units)
                {
                    UnitCalculator.CalculateLvUp(unit.lv, unit.exp, perUnit,
                        out int lv, out int exp, out int totalExp, out _);

                    transitions.Add(UnitTransition.Create(unit, lv, exp, totalExp));
                    unit.lv = lv;
                    unit.exp = exp;

                    updated = true;
                }
            }

            return transitions;
        }

        private void UpdateRecord(GameRecordDto record, IGameEntity entity)
        {
            switch (entity.Type)
            {
                case GameType.Stage:
                    if (record.lastClearedStage >= entity.Id)
                        ServiceLocator.Get<CastleHero.Data.Repositories.IUserRepository>().StageFocus = entity.Id;

                    record.lastClearedStage = Mathf.Max(record.lastClearedStage, entity.Id);
                    break;
                case GameType.Dungeon:
                    if (entity is not DungeonEntity dungeonEntity)
                        return;

                    var dungeonRecord = GetLatestDungeonRecord(record, dungeonEntity);
                    if (dungeonRecord == null)
                    {
                        dungeonRecord = new DungeonRecord { layer = dungeonEntity.Layer };

                        record.dungeon.Add(dungeonRecord);
                    }

                    dungeonRecord.lastClearedLv = Mathf.Max(dungeonRecord.lastClearedLv, dungeonEntity.Lv);
                    break;
            }
        }

        private bool IsFirstClear(GameRecordDto record, IGameEntity entity)
        {
            int lv = entity.Type switch
            {
                GameType.Stage => GetLatestStageLv(record),
                GameType.Dungeon => GetLatestDungeonRecord(record, (DungeonEntity)entity)?.lastClearedLv ?? 0,
                _ => -1
            };

            return lv + 1 == entity.Lv;
        }

        private int GetLatestStageLv(GameRecordDto record) => record.lastClearedStage;

        private DungeonRecord GetLatestDungeonRecord(GameRecordDto record, DungeonEntity entity) =>
            record.dungeon.FirstOrDefault(x => x.layer == entity.Layer);
    }
}
