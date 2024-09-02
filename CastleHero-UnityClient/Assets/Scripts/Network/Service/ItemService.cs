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
    public class ItemService : NetworkServiceBase
    {
        public async UniTask<Result<OpenBox>> OpenChest(int chestId, int quantity)
        {
            if (!Storage.db.items.TryFind(chestId, out var entity))
                return Result<OpenBox>.Error(Error.DataNotFound);

            var option = entity.GetChestOption(false);
            (CurrencyDto currency, List<IItem> items) reward = (new(), new());
            for (int i = 0; i < quantity; ++i)
            {
                var quantities = option.min
                    .Select((min, index) => Random.Range(min, option.max[index] + 1))
                    .ToArray();
                
                var temp = ItemGen.NewItems(option.Ids, quantities);
                reward.currency += temp.currency;
                reward.items.AddOrNew(temp.items);
            }

            var readTables = new List<Table> { Table.Inventory };
            var writeTables = new Dictionary<Table, object>();
            Action<UserDataDto> onAfterRead = null;
            if (reward.items.Count > 0)
            {
                onAfterRead += (userData) => { userData.inventory.items.AddOrNew(reward.items); };
            }

            if (!reward.currency.IsEmpty())
            {
                readTables.Add(Table.Currency);
                onAfterRead += (userData) =>
                {
                    userData.currency += reward.currency;
                    writeTables.Add(Table.Currency, userData.currency);
                };
            }

            var get = await GetTables(readTables);
            if (!get.IsSuccess)
                return Result<OpenBox>.Error(get.error);

            var userData = get.data;
            if (!userData.inventory.items.TryConsumeItem(chestId, quantity))
                return Result<OpenBox>.Error(Error.NotEnoughItem);

            writeTables.Add(Table.Inventory, userData.inventory);

            onAfterRead?.Invoke(userData);

            var update = await UpdateTables(writeTables);

            if (!update.IsSuccess)
                return Result<OpenBox>.Error(update.error);

            return Result<OpenBox>.Complete(new OpenBox
            {
                currency = reward.currency,
                items = reward.items,
            });
        }

        public async UniTask<Result<int>> Sell(IItem[] items, int[] quantities)
        {
            var get = await GetTables(Table.Currency, Table.Character, Table.Inventory);
            if (!get.IsSuccess)
                return Result<int>.Error(get.error);

            var userData = get.data;
            var characters = userData.characters;
            var inventory = userData.inventory;
            var currency = userData.currency;

            int index = 0;
            int gold = 0;
            bool updateCharacters = false;
            while (index.IsValidIndex(items, quantities))
            {
                var item = items[index];
                int quantity = quantities[index];
                ++index;

                if (!Storage.db.items.TryFind(item.ItemId, out var entity))
                    continue;

                if (item is EquipItem equipItem)
                {
                    var unit = characters.units.Find(x =>
                        x.equipments != null && x.equipments.Contains(equipItem.Guid));
                    int itemIndex = inventory.items.FindIndex(x => (x is EquipItem e) && e.Guid == equipItem.Guid);
                    if (!itemIndex.IsValidIndex(inventory.items))
                        return Result<int>.Error(Error.InvalidRequest);

                    if (unit != null)
                    {
                        unit.equipments.Remove(equipItem.Guid);
                        updateCharacters = true;
                    }

                    inventory.items.RemoveAt(itemIndex);
                    gold += entity.sellPrice;
                }
                else
                {
                    if(!inventory.items.TryConsumeItem(item, quantity))
                        return Result<int>.Error(Error.InvalidRequest);

                    gold += quantity * entity.sellPrice;
                }
            }

            var tables = new Dictionary<Table, object>();
            if (gold > 0)
            {
                currency.gold += gold;
                tables.Add(Table.Currency, currency);
            }

            if (updateCharacters)
            {
                tables.Add(Table.Character, characters);
            }

            var update = await UpdateTables(tables);
            return update.IsSuccess
                ? Result<int>.Complete(gold)
                : Result<int>.Error(update.error);
        }

        private async UniTask<Result<StaminaDto>> AddStamina(UserDataDto userData, int id, int amount)
        {
            var inventory = userData.inventory;
            var stamina = userData.stamina;

            if (!Storage.db.items.TryFind(id, out var entity))
                return Result<StaminaDto>.Error(Error.DataNotFound);

            var option = entity.GetConsumableOption();
            if (option.type != ConsumeType.Ap || !inventory.items.TryConsumeItem(id, amount))
                return Result<StaminaDto>.Error(Error.InvalidRequest);

            var add = await AddStamina(stamina, amount * (int)option.value);
            if (!add.IsSuccess)
                return Result<StaminaDto>.Error(add.error);

            var update = await UpdateTables(userData);
            return update.IsSuccess
                ? Result<StaminaDto>.Complete(stamina)
                : Result<StaminaDto>.Error(update.error);
        }
    }
}