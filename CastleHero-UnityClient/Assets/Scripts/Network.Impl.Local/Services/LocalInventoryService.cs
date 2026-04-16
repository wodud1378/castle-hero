using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using Random = UnityEngine.Random;

namespace CastleHero.Network.Impl.Local.Services
{
    public class LocalInventoryService : LocalNetworkServiceBase, IInventoryService
    {
        public LocalInventoryService(LocalUserDataStore store) : base(store) { }

        public UniTask<Result<OpenBox>> OpenChest(int chestId, int quantity)
        {
            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(chestId, out var entity))
                return UniTask.FromResult(Result<OpenBox>.Error(Error.DataNotFound));

            var option = entity.optionChest;
            (CurrencyDto currency, List<IItem> items) reward = (new(), new());
            for (int i = 0; i < quantity; ++i)
            {
                var quantities = option.min
                    .Select((min, index) => Random.Range(min, option.max[index] + 1))
                    .ToArray();

                var temp = ItemGen.NewItems(option.Ids, quantities);
                reward.currency += temp.currency;
                reward.items.Join(temp.items);
            }

            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<OpenBox>.Error(get.error));

            var userData = get.data;
            if (!userData.inventory.items.TryConsumeItem(chestId, quantity))
                return UniTask.FromResult(Result<OpenBox>.Error(Error.NotEnoughItem));

            if (reward.items.Count > 0)
                userData.inventory.items.Join(reward.items);

            if (!reward.currency.IsEmpty())
                userData.currency += reward.currency;

            Save(userData);

            return UniTask.FromResult(Result<OpenBox>.Complete(new OpenBox
            {
                currency = reward.currency,
                items = reward.items,
            }));
        }

        public UniTask<Result<int>> Sell(IItem[] items, int[] quantities)
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<int>.Error(get.error));

            var userData = get.data;
            var characters = userData.characters;
            var inventory = userData.inventory;
            var currency = userData.currency;

            int index = 0;
            int gold = 0;
            while (index.IsValidIndex(items, quantities))
            {
                var item = items[index];
                int quantity = quantities[index];
                ++index;

                if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(item.ItemId, out var entity))
                    continue;

                if (item is EquipItem equipItem)
                {
                    var unit = characters.units.Find(x =>
                        x.equipments != null && x.equipments.Contains(equipItem.Guid));
                    int itemIndex = inventory.items.FindIndex(x => (x is EquipItem e) && e.Guid == equipItem.Guid);
                    if (!itemIndex.IsValidIndex(inventory.items))
                        return UniTask.FromResult(Result<int>.Error(Error.InvalidRequest));

                    unit?.equipments.Remove(equipItem.Guid);

                    inventory.items.RemoveAt(itemIndex);
                    gold += entity.sellPrice;
                }
                else
                {
                    if (!inventory.items.TryConsumeItem(item, quantity))
                        return UniTask.FromResult(Result<int>.Error(Error.InvalidRequest));

                    gold += quantity * entity.sellPrice;
                }
            }

            if (gold <= 0)
                return UniTask.FromResult(Result<int>.Error(Error.InvalidRequest));

            currency.gold += gold;

            Save(userData);

            return UniTask.FromResult(Result<int>.Complete(gold));
        }

        public UniTask<Result<List<IItem>>> Combine(int id, int quantity)
        {
            var error = TryGetIngredientData(id, out _, out var option);
            if (error != Error.None)
                return UniTask.FromResult(Result<List<IItem>>.Error(error));

            if (option.type == IngredientType.Soul)
                return UniTask.FromResult(Result<List<IItem>>.Error(Error.InvalidRequest));

            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<List<IItem>>.Error(get.error));

            var userData = get.data;
            var inventory = userData.inventory;
            int consume = quantity * option.forCombine;
            if (quantity == 0 || !inventory.items.TryConsumeItem(id, consume))
                return UniTask.FromResult(Result<List<IItem>>.Error(Error.NotEnoughItem));

            var result = ItemGen.NewItems(option.targetId, quantity).items;
            inventory.items.Join(result);

            Save(userData);

            return UniTask.FromResult(Result<List<IItem>>.Complete(result));
        }

        private Error TryGetIngredientData(int id, out ItemEntity entity, out IngredientOption option)
        {
            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(id, out entity))
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

        public UniTask<Result<StaminaDto>> AddStamina(int id, int amount)
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<StaminaDto>.Error(get.error));

            var userData = get.data;
            var error = TryGetConsumableData(id, out _, out var option);
            if (error != Error.None)
                return UniTask.FromResult(Result<StaminaDto>.Error(error));

            var inventory = userData.inventory;
            var stamina = userData.stamina;
            if (option.type != ConsumeType.Stamina || !inventory.items.TryConsumeItem(id, amount))
                return UniTask.FromResult(Result<StaminaDto>.Error(Error.InvalidRequest));

            RecoverStamina(stamina);
            stamina.point += amount * (int)option.value;

            Save(userData);

            return UniTask.FromResult(Result<StaminaDto>.Complete(stamina));
        }

        private Error TryGetConsumableData(int id, out ItemEntity entity, out ConsumableOption option)
        {
            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(id, out entity))
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

        public UniTask<Result<EquipItem>> Refine(string guid, int itemId)
        {
            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(itemId, out var entity) ||
                !entity.TryGetElementalOption(out var option))
                return UniTask.FromResult(Result<EquipItem>.Error(Error.DataNotFound));

            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result<EquipItem>.Error(get.error));

            var userData = get.data;
            var inventory = userData.inventory;
            var item = inventory.items.Find(x => x.ItemId == itemId);
            var equipItem = inventory.items.OfType<EquipItem>().FirstOrDefault(x => x.Guid == guid);
            if (equipItem == null ||
                item == null ||
                (EquipmentSlot)equipItem.slot is not EquipmentSlot.Weapon and EquipmentSlot.Armor)
                return UniTask.FromResult(Result<EquipItem>.Error(Error.InvalidRequest));

            if (!inventory.items.TryConsumeItem(itemId, 1))
                return UniTask.FromResult(Result<EquipItem>.Error(Error.NotEnoughItem));

            equipItem.element = new EquipItem.Element
            {
                type = (int)option.type,
                lv = option.lv
            };

            Save(userData);

            return UniTask.FromResult(Result<EquipItem>.Complete(equipItem));
        }
    }
}
