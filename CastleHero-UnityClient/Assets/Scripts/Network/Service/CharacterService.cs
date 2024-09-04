using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Utility;

namespace RGLabs.Network.Service
{
    public class CharacterService : NetworkServiceBase
    {
        public enum GrowthAction
        {
            Lv,
            Rate
        }
        public async UniTask<Result<UnitGrowth>> Growth(GrowthAction action, int unitId, int itemId, int quantity)
        {
            var read = await GetTables(Table.Character, Table.Inventory, Table.Currency);
            if (!read.IsSuccess)
                return Result<UnitGrowth>.Error(read.error);

            var characters = read.data.characters;
            var inventory = read.data.inventory;
            var currency = read.data.currency;

            var unit = characters.units.Find(x => x.id == unitId);
            var item = inventory.items.Find(x => x.ItemId == itemId);
            var error = Error.InvalidRequest;
            var transition = action switch
            {
                GrowthAction.Lv => ProcessLvUp(unit, item, quantity, currency, out error),
                GrowthAction.Rate => ProcessUpgrade(unit, item, quantity, currency, out error),
                _=> null
            };
            
            if (error != Error.None)
                return Result<UnitGrowth>.Error(error);
            
            if(!inventory.items.TryConsumeItem(item, quantity))
                return Result<UnitGrowth>.Error(Error.NotEnoughItem);

            var write = await UpdateTables(new Dictionary<Table, object>
            {
                { Table.Character, characters },
                { Table.Inventory, inventory },
                { Table.Currency, currency },
            });

            return write.IsSuccess
                ? Result<UnitGrowth>.Complete(new()
                {
                    transition = transition,
                    leftCurrency = currency,
                    leftItem = item
                })
                : Result<UnitGrowth>.Error(write.error);
        }

        private UnitTransition ProcessUpgrade(UnitInfo unit, IItem item, int quantity, CurrencyDto currency,
            out Error error)
        {
            if (Storage.db.rates.MaxRate == unit.rate)
            {
                error = Error.AlreadyMaxLv;
                return null;
            }

            if (!Storage.db.items.TryFind(item.ItemId, out var itemEntity))
            {
                error = Error.DataNotFound;
                return null;
            }
            
            UnitHelper.CalculateUpgrade(unit.id, unit.rate, itemEntity, quantity,
                out int rate, out int leftItem, out int price);
            
            if (currency.gold < price)
            {
                error = Error.NotEnoughCurrency;
                return null;
            }
            
            if (unit.rate == rate)
            {
                error = Error.InvalidRequest;
                return null;
            }
            
            var transition = UnitTransition.Create(unit, rate);

            unit.rate = rate;

            error = Error.None;

            return transition;
        }

        private UnitTransition ProcessLvUp(UnitInfo unit, IItem item, int quantity, CurrencyDto currency,
            out Error error)
        {
            if (Storage.db.levels.MaxLv == unit.lv)
            {
                error = Error.AlreadyMaxLv;
                return null;
            }

            if (!Storage.db.items.TryFind(item.ItemId, out var itemEntity))
            {
                error = Error.DataNotFound;
                return null;
            }

            if (itemEntity.optionConsume.type != ConsumeType.Exp)
            {
                error = Error.InvalidRequest;
                return null;
            }

            UnitHelper.CalculateLvUp(unit.lv, unit.exp, itemEntity, quantity,
                out int lv, out int exp, out int leftItem, out int price);

            if (currency.gold < price)
            {
                error = Error.NotEnoughCurrency;
                return null;
            }
            
            if(unit.lv == lv && unit.exp == exp)
            {
                error = Error.InvalidRequest;
                return null;
            }

            var transition = UnitTransition.Create(unit, lv, exp);

            unit.lv = lv;
            unit.exp = exp;
            item.Quantity = leftItem;

            error = Error.None;
            
            return transition;
        }

        public async UniTask<Result<UnitInfo>> Equip(int unitId, string guid)
        {
            var get = await GetTables(Table.Character, Table.Inventory);
            if (!get.IsSuccess)
                return Result<UnitInfo>.Error(get.error);

            var userData = get.data;
            var characters = userData.characters;
            var inventory = userData.inventory;
            var unit = characters.units.Find(x => x.id == unitId);
            var equipItems = inventory.items.OfType<EquipItem>().ToList();
            var item = equipItems.FirstOrDefault(x => x.Guid == guid);
            if (unit == null || item == null || (unit.equipments != null && unit.equipments.Contains(guid)))
                return Result<UnitInfo>.Error(Error.InvalidRequest);

            unit.equipments ??= new();
            
            if (item.character != 0)
            {
                var last = characters.units.Find(x => x.id == item.character);
                last.equipments.Remove(guid);
            }

            var exist = equipItems
                .FirstOrDefault(x => unit.equipments.Contains(x.Guid) && x.slot == item.slot);

            if (exist != null)
            {
                exist.character = 0;
                unit.equipments.Remove(exist.Guid);
            }

            item.character = unit.id;
            unit.equipments.Add(item.Guid);

            var update = await UpdateTables(userData);
            return update.IsSuccess
                ? Result<UnitInfo>.Complete(unit)
                : Result<UnitInfo>.Error(update.error);
        }

        public async UniTask<Result<UnitInfo>> Release(int unitId, string guid)
        {
            var get = await GetTables(Table.Character, Table.Inventory);
            if (!get.IsSuccess)
                return Result<UnitInfo>.Error(get.error);
            
            var userData = get.data;
            var characters = userData.characters;
            var inventory = userData.inventory;
            var unit = characters.units.Find(x => x.id == unitId);
            var item = inventory.items.OfType<EquipItem>().FirstOrDefault(x => x.Guid == guid);
            if (unit == null || item == null || unit.equipments == null || !unit.equipments.Contains(guid))
                return Result<UnitInfo>.Error(Error.InvalidRequest);

            item.character = 0;
            unit.equipments.Remove(item.Guid);
            
            var update = await UpdateTables(userData);
            return update.IsSuccess
                ? Result<UnitInfo>.Complete(unit)
                : Result<UnitInfo>.Error(update.error);
        }
    }
}