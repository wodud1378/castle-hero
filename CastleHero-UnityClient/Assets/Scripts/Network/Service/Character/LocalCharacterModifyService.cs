using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;

namespace RGLabs.Network.Service.Character
{
    public class LocalCharacterModifyService : ICharacterModifyService
    {
        public UniTask<LevelUpResult> LevelUp(UnitInfo unit, ConsumableItem item, int consumeQuantity)
        {
            int gold = Storage.userRepository.gold.Value;
            var failedCause = IModifyCharacter.FailedCauses.None;

            if (!TryCalculateLvUp(item, consumeQuantity, unit.lv, unit.exp,
                    out int endLv, out int endExp, out int requireGold))
                failedCause = IModifyCharacter.FailedCauses.Unknown;
            else if (gold < requireGold || item.Quantity < consumeQuantity)
                failedCause = IModifyCharacter.FailedCauses.NotEnoughItem;
            else if (unit.lv == endLv)
                failedCause = IModifyCharacter.FailedCauses.AlreadyMaxValue;

            if (failedCause != IModifyCharacter.FailedCauses.None)
            {
                var characters = Storage.userRepository.characters;
                var items = Storage.userRepository.items;
                int unitIndex = characters.IndexOf(unit);
                int itemIndex = items.IndexOf(item);

                unit.lv = endLv;
                unit.exp = endExp;

                item.Quantity -= consumeQuantity;

                characters.RemoveAt(unitIndex);
                characters.Insert(unitIndex, unit);

                items.RemoveAt(itemIndex);
                items.Insert(itemIndex, item);

                gold -= requireGold;
                Storage.userRepository.gold.Value = gold;
            }

            return UniTask.FromResult(new LevelUpResult
            {
                FailedCause = failedCause,
                Info = unit,
                ItemResult = item,
                GoldResult = gold,
            });
        }

        private bool TryCalculateLvUp(ConsumableItem item, int quantity, int startLv, int startExp, 
            out int endLv, out int endExp, out int requireGold)
        {
            endLv = startLv;
            endExp = startExp;
            requireGold = 0;

            if (!Storage.db.itemDBAccessor.TryLoad(item.ItemId, out var e) ||
                e is not ConsumableEntity itemEntity)
                return false;

            endExp = quantity * itemEntity.optionValue;
            var db = Storage.db.levels;
            int lv = startLv;
            while (db.TryFind(lv++, out var entity) && endExp - entity.exp > 0)
            {
                endLv = entity.Id + 1;
                requireGold += entity.gold;

                endExp -= entity.exp;
            }

            return true;
        }

        public UniTask<UpgradeResult> Upgrade(UnitInfo unit, ConsumableItem item, int consumeQuantity)
        {
            int gold = Storage.userRepository.gold.Value;
            var failedCause = IModifyCharacter.FailedCauses.None;
            
            if (!TryCalculateUpgrade(item, unit.rate,
                    out int endRate, out int requireGold))
                failedCause = IModifyCharacter.FailedCauses.NotEnoughItem;
            else if (gold < requireGold || item.Quantity < consumeQuantity)
                failedCause = IModifyCharacter.FailedCauses.NotEnoughItem;
            else if (unit.rate == endRate)
                failedCause = IModifyCharacter.FailedCauses.AlreadyMaxValue;

            if (failedCause != IModifyCharacter.FailedCauses.None)
            {
                var characters = Storage.userRepository.characters;
                var items = Storage.userRepository.items;
                int unitIndex = characters.IndexOf(unit);
                int itemIndex = items.IndexOf(item);

                unit.rate = endRate;

                item.Quantity -= consumeQuantity;

                characters.RemoveAt(unitIndex);
                characters.Insert(unitIndex, unit);

                items.RemoveAt(itemIndex);
                items.Insert(itemIndex, item);

                gold -= requireGold;
                Storage.userRepository.gold.Value = gold;
            }

            return UniTask.FromResult(new UpgradeResult
            {
                FailedCause = failedCause,
                Info = unit,
                ItemResult = item,
                GoldResult = gold,
            });
        }

        private bool TryCalculateUpgrade(ConsumableItem item, int startRate, 
            out int endRate, out int requireGold)
        {
            endRate = startRate;
            requireGold = 0;

            if (!Storage.db.itemDBAccessor.TryLoad(item.ItemId, out var e) ||
                e is not ConsumableEntity itemEntity)
                return false;

            var db = Storage.db.rates;
            int qty = item.Quantity;
            int rate = startRate;
            while (db.TryFind(rate++, out var entity) && qty - entity.soul > 0)
            {
                endRate = entity.Id + 1;
                requireGold += entity.gold;

                qty -= entity.soul;
            }

            return true;
        }
    }
}