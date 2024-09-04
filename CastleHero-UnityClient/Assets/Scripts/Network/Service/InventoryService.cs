using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using Random = UnityEngine.Random;

namespace RGLabs.Network.Service
{
    public class InventoryService : NetworkServiceBase
    {
        public async UniTask<Result<OpenBox>> OpenChest(int chestId, int quantity)
        {
            if (!Storage.db.items.TryFind(chestId, out var entity))
                return Result<OpenBox>.Error(Error.DataNotFound);

            var option = entity.optionChest;
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
                    if (!inventory.items.TryConsumeItem(item, quantity))
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

        public async UniTask<Result<List<IItem>>> Combine(int id, int amount)
        {
            var error = TryGetIngredientData(id, out _, out var option);
            if(error != Error.None)
                return Result<List<IItem>>.Error(error);

            if(option.type == IngredientType.Soul)
                return Result<List<IItem>>.Error(Error.InvalidRequest);
            
            var get = await GetTables(Table.Inventory);
            if (!get.IsSuccess)
                return Result<List<IItem>>.Error(get.error);

            var userData = get.data;
            var inventory = userData.inventory;
            int quantity = amount / option.forCombine;
            int consume = quantity * option.forCombine; 
            if(quantity == 0 || !inventory.items.TryConsumeItem(id, consume))
                return Result<List<IItem>>.Error(Error.NotEnoughItem);

            var result = ItemGen.NewItems(option.targetId, quantity).items;
            inventory.items.AddOrNew(result);
            
            var update = await UpdateTables(userData);
            return update.IsSuccess
                ? Result<List<IItem>>.Complete(result)
                : Result<List<IItem>>.Error(update.error);
        }
        
        private Error TryGetIngredientData(int id, out ItemEntity entity, out IngredientOption option)
        {
            if (!Storage.db.items.TryFind(id, out entity))
            {
                option = default;
                return Error.DataNotFound;
            }

            if (entity.type != ItemType.Ingredient)
            {
                option = default;
                return Error.InvalidRequest;
            }

            option = entity.optionIngredient;
            return Error.None;
        }

        public async UniTask<Result<StaminaDto>> AddStamina(int id, int amount)
        {
            var get = await GetTables(Table.Stamina);
            if (!get.IsSuccess)
                return Result<StaminaDto>.Error(get.error);

            var userData = get.data;
            var error = TryGetConsumableData(id, out var entity, out var option);
            if(error != Error.None)
                return Result<StaminaDto>.Error(error);
            
            var inventory = userData.inventory;
            var stamina = userData.stamina;
            if (option.type != ConsumeType.Stamina || !inventory.items.TryConsumeItem(id, amount))
                return Result<StaminaDto>.Error(Error.InvalidRequest);

            var add = await AddStamina(stamina, amount * (int)option.value);
            if (!add.IsSuccess)
                return Result<StaminaDto>.Error(add.error);

            var update = await UpdateTables(userData);
            return update.IsSuccess
                ? Result<StaminaDto>.Complete(stamina)
                : Result<StaminaDto>.Error(update.error);
        }

        private Error TryGetConsumableData(int id, out ItemEntity entity, out ConsumableOption option)
        {
            if (!Storage.db.items.TryFind(id, out entity))
            {
                option = default;
                return Error.DataNotFound;
            }

            if (entity.type != ItemType.Consumable)
            {
                option = default;
                return Error.InvalidRequest;
            }

            option = entity.optionConsume;
            return Error.None;
        }

        public async UniTask<Result<EquipItem>> Refine(string guid, int itemId)
        {
            if (!Storage.db.items.TryFind(itemId, out var entity) ||
                !entity.TryGetElementalOption(out var option))
                return Result<EquipItem>.Error(Error.DataNotFound);
            
            var get = await GetTables(Table.Inventory);
            if (!get.IsSuccess)
                return Result<EquipItem>.Error(get.error);

            var userData = get.data;
            var inventory = userData.inventory;
            var item = inventory.items.Find(x => x.ItemId == itemId);
            var equipItem = inventory.items.OfType<EquipItem>().FirstOrDefault(x => x.Guid == guid);
            if (equipItem == null ||
                item == null ||
                (EquipmentSlot)equipItem.slot is not EquipmentSlot.Weapon and EquipmentSlot.Armor)
                return Result<EquipItem>.Error(Error.InvalidRequest);

            if (!inventory.items.TryConsumeItem(itemId, 1))
                return Result<EquipItem>.Error(Error.NotEnoughItem);

            equipItem.element = new EquipItem.Element
            {
                type = (int)option.type,
                lv = option.lv
            };

            var update = await UpdateTables(userData);
            return update.IsSuccess
                ? Result<EquipItem>.Complete(equipItem)
                : Result<EquipItem>.Error(update.error);
        }
    }
}