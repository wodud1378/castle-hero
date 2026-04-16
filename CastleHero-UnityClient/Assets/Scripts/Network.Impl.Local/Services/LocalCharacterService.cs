using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;

namespace CastleHero.Network.Impl.Local.Services
{
    public class LocalCharacterService : LocalNetworkServiceBase, ICharacterService
    {
        public LocalCharacterService(LocalUserDataStore store) : base(store) { }

        public UniTask<Result<UnitGrowth>> Growth(GrowthAction action, int unitId, int itemId, int quantity)
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<UnitGrowth>.Error(get.error));

            var userData = get.data;
            var characters = userData.characters;
            var inventory = userData.inventory;
            var currency = userData.currency;

            var unit = characters.units.Find(x => x.id == unitId);
            var item = inventory.items.Find(x => x.ItemId == itemId);
            var error = Error.InvalidRequest;
            int leftItem = quantity;
            var transition = action switch
            {
                GrowthAction.Lv => ProcessLvUp(unit, item, quantity, currency, out leftItem, out error),
                GrowthAction.Rate => ProcessUpgrade(unit, item, quantity, currency, out leftItem, out error),
                _ => null
            };

            if (error != Error.None)
                return UniTask.FromResult(Result<UnitGrowth>.Error(error));

            if (!inventory.items.TryConsumeItem(item, quantity - leftItem))
                return UniTask.FromResult(Result<UnitGrowth>.Error(Error.NotEnoughItem));

            Save(userData);

            return UniTask.FromResult(Result<UnitGrowth>.Complete(new UnitGrowth
            {
                transition = transition,
                leftCurrency = currency,
                leftItem = item
            }));
        }

        private UnitTransition ProcessUpgrade(UnitInfo unit, IItem item, int quantity, CurrencyDto currency,
            out int leftItem, out Error error)
        {
            leftItem = quantity;
            if (unit == null || item == null)
            {
                error = Error.InvalidRequest;
                return null;
            }

            if (ServiceLocator.Get<IDBProvider>().Rates.MaxRate == unit.rate)
            {
                error = Error.AlreadyMaxLv;
                return null;
            }

            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(item.ItemId, out var itemEntity))
            {
                error = Error.DataNotFound;
                return null;
            }

            UnitCalculator.CalculateUpgrade(unit.id, unit.rate, itemEntity, quantity,
                out int rate, out leftItem, out int price);

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
            currency.gold -= price;

            error = Error.None;

            return transition;
        }

        private UnitTransition ProcessLvUp(UnitInfo unit, IItem item, int quantity, CurrencyDto currency,
            out int leftItem, out Error error)
        {
            leftItem = quantity;
            if (unit == null || item == null)
            {
                error = Error.InvalidRequest;
                return null;
            }

            if (ServiceLocator.Get<IDBProvider>().Levels.MaxLv == unit.lv)
            {
                error = Error.AlreadyMaxLv;
                return null;
            }

            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(item.ItemId, out var itemEntity))
            {
                error = Error.DataNotFound;
                return null;
            }

            if (itemEntity.optionConsume.type != ConsumeType.Exp)
            {
                error = Error.InvalidRequest;
                return null;
            }

            UnitCalculator.CalculateLvUp(unit.lv, unit.exp, itemEntity, quantity,
                out int lv, out int exp, out int totalExp, out leftItem, out int price);

            if (currency.gold < price)
            {
                error = Error.NotEnoughCurrency;
                return null;
            }

            if (unit.lv == lv && unit.exp == exp)
            {
                error = Error.InvalidRequest;
                return null;
            }

            var transition = UnitTransition.Create(unit, lv, exp, totalExp);

            unit.lv = lv;
            unit.exp = exp;
            currency.gold -= price;

            error = Error.None;

            return transition;
        }

        public UniTask<Result<UnitInfo>> Equip(int unitId, string guid)
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<UnitInfo>.Error(get.error));

            var userData = get.data;
            var characters = userData.characters;
            var inventory = userData.inventory;
            var unit = characters.units.Find(x => x.id == unitId);
            var equipItems = inventory.items.OfType<EquipItem>().ToList();
            var item = equipItems.FirstOrDefault(x => x.Guid == guid);
            if (unit == null || item == null || (unit.equipments != null && unit.equipments.Contains(guid)))
                return UniTask.FromResult(Result<UnitInfo>.Error(Error.InvalidRequest));

            unit.equipments ??= new();

            if (item.character != 0)
            {
                var last = characters.units.Find(x => x.id == item.character);
                last?.equipments.Remove(guid);
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

            Save(userData);

            return UniTask.FromResult(Result<UnitInfo>.Complete(unit));
        }

        public UniTask<Result<UnitInfo>> Release(int unitId, string guid)
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<UnitInfo>.Error(get.error));

            var userData = get.data;
            var characters = userData.characters;
            var inventory = userData.inventory;
            var unit = characters.units.Find(x => x.id == unitId);
            var item = inventory.items.OfType<EquipItem>().FirstOrDefault(x => x.Guid == guid);
            if (unit == null || item == null || unit.equipments == null || !unit.equipments.Contains(guid))
                return UniTask.FromResult(Result<UnitInfo>.Error(Error.InvalidRequest));

            item.character = 0;
            unit.equipments.Remove(item.Guid);

            Save(userData);

            return UniTask.FromResult(Result<UnitInfo>.Complete(unit));
        }
    }
}
