using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
        public async UniTask<Result<List<DungeonType>>> GetOpenedDungeonTypes()
        {
            return Result<List<DungeonType>>.Complete(new List<DungeonType>
            {
                DungeonType.Assault,
                DungeonType.Escort,
                DungeonType.Raid,
                DungeonType.Invasion,
            });

            // var map = new Dictionary<DungeonType, List<DayOfWeek>>();
            // Storage.db.dungeons.BinarySearch(x =>
            // {
            //     if (map.ContainsKey(x.type))
            //         return;
            //
            //     map.Add(x.type, x.OpenDaysOfWeek());
            // });
            //
            // var getTime = await GetServerTime();
            // if (!getTime.IsSuccess)
            //     return Result<List<DungeonType>>.Error(getTime.error);
            //
            // var dow = getTime.data.DayOfWeek;
            // var list = map
            //     .Where(x => x.Value.Contains(dow))
            //     .Select(x => x.Key).ToList();
            //
            // return Result<List<DungeonType>>.Complete(list);
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

            var getTime = await GetServerTime();
            if (!getTime.IsSuccess)
                return getTime.error;

            var record = get.data.gameRecord;
            var checkIsOpened = await CheckIsOpened(record, getTime.data, entity);
            return !checkIsOpened.IsSuccess ? checkIsOpened.error : Error.None;
        }

        private async UniTask<Error> CanClear(UserDataDto userData, DateTime currentTime, IGameEntity entity)
        {
            var stamina = userData.stamina;
            var update = await UpdateStamina(stamina);
            if (!update.IsSuccess)
                return update.error;

            if (stamina.point < entity.Ap)
                return Error.NotEnoughAp;

            var record = userData.gameRecord;
            var checkIsOpened = await CheckIsOpened(record, currentTime, entity);
            return !checkIsOpened.IsSuccess ? checkIsOpened.error : Error.None;
        }

        private async UniTask<Result> CheckIsOpened(GameRecordDto record, DateTime currentTime, IGameEntity entity)
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
                    var getOpenedTypes = await GetOpenedDungeonTypes();
                    if (!getOpenedTypes.IsSuccess)
                        return Result.Error(getOpenedTypes.error);

                    if (!getOpenedTypes.data.Contains(dungeonEntity.type))
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

            var currentTime = DateTime.MinValue;
            if (type == GameType.Dungeon)
            {
                var getTime = await GetServerTime();
                if (!getTime.IsSuccess)
                    return Result<GameCleared>.Error(getTime.error);

                currentTime = getTime.data;
            }

            var error = await CanClear(userData, currentTime, entity);
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
            
            UpdateRecord(userData.gameRecord, currentTime, entity);

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

        private void UpdateRecord(GameRecordDto record, DateTime currentTime, IGameEntity entity)
        {
            switch (entity.Type)
            {
                case GameType.Stage:
                    record.lastClearedStage = Mathf.Max(record.lastClearedStage, entity.Id);
                    break;
                case GameType.Dungeon:
                    if (entity is not DungeonEntity dungeonEntity)
                        return;

                    var dungeonRecord = record.dungeon.Find(x =>
                        x.type == (int)dungeonEntity.type && x.detailType == (int)dungeonEntity.detailType);
                    
                    if (dungeonRecord == null)
                    {
                        dungeonRecord = new DungeonRecord
                        {
                            type = (int)dungeonEntity.type,
                            detailType = (int)dungeonEntity.detailType,
                        };
                        
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

        private DungeonRecord GetLatestDungeonRecord(GameRecordDto record, DungeonEntity entity)
            => record.dungeon
                .FirstOrDefault(x => x.type == (int)entity.type && x.detailType == (int)entity.detailType);
    }
}