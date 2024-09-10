using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.Network.Service
{
    public class GameService : NetworkServiceBase
    {
        public async UniTask<Result<List<int>>> GetOpenedDungeonLayers()
        {
            var map = new Dictionary<int, List<DayOfWeek>>();
            Storage.db.dungeons.BinarySearch(x =>
            {
                if (map.ContainsKey(x.Layer))
                    return;

                map.Add(x.Layer, x.OpenDaysOfWeek());
            });

            async UniTask<Result<List<int>>> Filter(Dictionary<int, List<DayOfWeek>> collection)
            {
                var getTime = await GetServerTime();
                if (!getTime.IsSuccess)
                    return Result<List<int>>.Error(getTime.error);

                var dow = getTime.data.DayOfWeek;
                return Result<List<int>>.Complete(collection
                    .Where(x => x.Value.Contains(dow))
                    .Select(x => x.Key).ToList());
            }

#if UNITY_EDITOR
            if (Context.NetworkConfig.openAllDungeons)
                return Result<List<int>>.Complete(map.Keys.ToList());
#endif
            return await Filter(map);
        }

        public async UniTask<Result> Start(GameType type, int id)
        {
            var error = await CanEntrance(type, id);
            if (error != Error.None)
                return Result.Error(error);

            return Result.Complete();
        }

        private async UniTask<Error> CanEntrance(GameType type, int id)
        {
            if (!Storage.db.TryLoadGameEntity(type, id, out var entity))
                return Error.DataNotFound;

            var get = await GetTables(Table.Stamina, Table.GameRecord);
            if (!get.IsSuccess)
                return get.error;

            var checkProcess = await HasEnoughAp(get.data.stamina, entity.Ap);
            if (!checkProcess.IsSuccess)
                return checkProcess.error;

            if (!checkProcess.data)
                return Error.NotEnoughAp;

            var record = get.data.gameRecord;
            var checkIsOpened = await CheckIsOpened(record, entity);
            return !checkIsOpened.IsSuccess ? checkIsOpened.error : Error.None;
        }

        private async UniTask<Error> CanClear(UserDataDto userData, IGameEntity entity)
        {
            var stamina = userData.stamina;
            var update = await UpdateStamina(stamina);
            if (!update.IsSuccess)
                return update.error;

            if (stamina.point < entity.Ap)
                return Error.NotEnoughAp;

            var record = userData.gameRecord;
            var checkIsOpened = await CheckIsOpened(record, entity);
            return !checkIsOpened.IsSuccess ? checkIsOpened.error : Error.None;
        }

        private async UniTask<Result> CheckIsOpened(GameRecordDto record, IGameEntity entity)
        {
            switch (entity.Type)
            {
                case GameType.Stage:
                    // 스테이지 진행도 체크.
                    return GetLatestStageLv(record) + 1 < entity.Lv ? Result.Error(Error.NotOpened) : Result.Complete();
                case GameType.Dungeon:
                    if (entity is not DungeonEntity dungeonEntity)
                        return Result.Error(Error.InvalidRequest);

                    // 열린 던전 타입인지 체크.
                    var getOpenedTypes = await GetOpenedDungeonLayers();
                    if (!getOpenedTypes.IsSuccess)
                        return Result.Error(getOpenedTypes.error);

                    if (!getOpenedTypes.data.Contains(dungeonEntity.Layer))
                        return Result.Error(Error.NotOpened);

                    // 해당 던전 기록 확인
                    var dungeonRecord = GetLatestDungeonRecord(record, dungeonEntity);
                    // 기본 레벨 0.
                    int lv = dungeonRecord?.lastClearedLv ?? 0;

                    // 최대 레벨 + 1 값을  넘어가는 던전인 경우 잠금 처리.
                    return lv + 1 < entity.Lv ? Result.Error(Error.NotOpened) : Result.Complete();
            }

            return Result.Error(Error.Unknown);
        }

        public async UniTask<Result<GameCleared>> Clear(GameType type, int id)
        {
            if (!Storage.db.TryLoadGameEntity(type, id, out var entity))
                return Result<GameCleared>.Error(Error.DataNotFound);

            var get = await GetTables();
            if (!get.IsSuccess)
                return Result<GameCleared>.Error(get.error);

            var userData = get.data;
            var error = await CanClear(userData, entity);
            if (error != Error.None)
                return Result<GameCleared>.Error(error);

            userData.stamina.point -= entity.Ap;

            var result = new GameCleared
            {
                id = entity.Id,
                exp = entity.Exp,
                isFirstClear = IsFirstClear(userData.gameRecord, entity),
                transitions = GetUnitTransition(
                    userData.formation, userData.characters, entity.Exp,
                    out bool updateCharacters)
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
                    return Result<GameCleared>.Error(Error.Unknown);
            }

            result.currency = reward.currency;
            result.items = reward.items;

            UpdateRecord(userData.gameRecord, entity);

            bool updateCurrency = !reward.currency.IsEmpty();
            bool updateInventory = reward.items.Count > 0;
            var tables = new Dictionary<Table, object>
            {
                { Table.Stamina, userData.stamina },
                { Table.GameRecord, userData.gameRecord },
            };
            if (updateCurrency)
            {
                userData.currency += reward.currency;
                tables.Add(Table.Currency, userData.currency);
            }

            if (updateInventory)
            {
                userData.inventory.items.Join(reward.items);
                tables.Add(Table.Inventory, userData.inventory);
            }

            if (updateCharacters)
            {
                tables.Add(Table.Character, userData.characters);
            }

            var update = await UpdateTables(tables);
            return update.IsSuccess
                ? Result<GameCleared>.Complete(result)
                : Result<GameCleared>.Error(update.error);
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
            if (Storage.db.dungeonRewards.TryFind(entity.rewardGroup, out var rewardEntity))
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
                .Where(unit => formation.fieldUnits.FindIndex(x => x.id == unit.id) != -1);

            if (addExp == 0)
            {
                foreach (var unit in units)
                {
                    transitions.Add(UnitTransition.Create(unit));
                }
            }
            else
            {
                foreach (var unit in units)
                {
                    UnitHelper.CalculateLvUp(unit.lv, unit.exp, addExp, out int lv, out int exp, out _);

                    transitions.Add(UnitTransition.Create(unit, lv, exp));
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