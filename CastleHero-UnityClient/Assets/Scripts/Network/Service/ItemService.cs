using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Network.Service
{
    public class ItemService: NetworkServiceBase
    {
        public async UniTask<Result<OpenBox>> OpenChest(int chestId, int quantity)
        {
            if (!Storage.db.items.TryFind(chestId, out var entity))
                return Result<OpenBox>.FromError(Error.DataNotFound);

            var option = entity.GetChestOption(false);
            (CurrencyDto currency, List<IItem> items) reward = (new(), new());
            for (int i = 0; i < quantity; ++i)
            {
                var temp = ItemGen.NewItems(option.Ids, option.quantities);
                reward.currency += temp.currency;
                reward.items.AddOrNew(temp.items);
            }

            var readTables = new List<Table> { Table.Inventory };
            var writeTables = new Dictionary<Table, object>();
            Action<UserDataDto> onAfterRead = null;
            if (reward.items.Count > 0)
            {
                onAfterRead += (userData) =>
                {
                    userData.inventory.items.AddOrNew(reward.items);
                };
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
                return Result<OpenBox>.FromError(get.error);

            var userData = get.data;
            if(!userData.inventory.items.TryConsumeItem(chestId, quantity))
                return Result<OpenBox>.FromError(Error.NotEnoughItem);

            writeTables.Add(Table.Inventory, userData.inventory);
            
            onAfterRead?.Invoke(userData);

            var update = await UpdateTables(writeTables);
            
            if(!update.IsSuccess)
                return Result<OpenBox>.FromError(update.error);

            return Result<OpenBox>.From(new OpenBox
            {
                currency = reward.currency,
                items = reward.items,
            });
        }

        public async UniTask<Result<int>> Sell(IItem[] items, int[] quantities)
        {
            var get = await GetTables(Table.Currency, Table.Character, Table.Inventory);
            if (!get.IsSuccess)
                return Result<int>.FromError(get.error);
         
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
                    var unit = characters.units.Find(x => x.equipments != null && x.equipments.Contains(equipItem.Guid));
                    int itemIndex = inventory.items.FindIndex(x => (x is EquipItem e) && e.Guid == equipItem.Guid);
                    if(!itemIndex.IsValidIndex(inventory.items))
                        return Result<int>.FromError(Error.InvalidRequest);

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
                    var exist = inventory.items.Find(x => x.ItemId == item.ItemId);
                    if(exist == null)
                        return Result<int>.FromError(Error.InvalidRequest);

                    int sellCount = Mathf.Min(exist.Quantity, quantity);
                    if (item.Quantity <= sellCount)
                        inventory.items.Remove(item);
                    else
                        exist.Quantity -= sellCount;
                    
                    gold += sellCount * entity.sellPrice;
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
                ? Result<int>.From(gold)
                : Result<int>.FromError(update.error);
        }
    }
}