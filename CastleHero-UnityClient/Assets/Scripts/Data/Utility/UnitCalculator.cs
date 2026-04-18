using System;
using CastleHero.Data;
using CastleHero.Data.Model;
using UnityEngine;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.Utility
{
    public static class UnitCalculator
    {
        private static IDBProvider _db;

        static UnitCalculator()
        {
            var sl = ServiceLocator.Instance;
            if (sl.TryGet<IDBProvider>(out var db)) _db = db;
            sl.OnRegistered += (type, instance) =>
            {
                if (type == typeof(IDBProvider)) _db = (IDBProvider)instance;
            };
        }

        public static void CalculateLvUp(int startLv, int startExp, ItemEntity item, int quantity,
            out int lv, out int exp, out int totalExp, out int leftItem, out int price)
        {
            lv = startLv;
            exp = startExp;
            leftItem = quantity;
            totalExp = 0;
            price = 0;

            var option = item.optionConsume;
            if (option.type != ConsumeType.Exp)
                return;

            int amount = (int)option.value;
            int left = amount * quantity;

            var db = _db.Levels;
            int maxLv = db.MaxLv;
            while (db.TryFind(lv, out var entity) && left > 0)
            {
                int forNext = entity.exp;
                int requireExp = forNext - exp;
                int add = Mathf.Min(requireExp, left);
                exp += add;
                totalExp += add;
                price += entity.gold;
                left -= add;

                int remain = exp - forNext;
                if (remain >= 0)
                {
                    ++lv;
                    exp = lv >= maxLv ? 0 : remain;
                }
            }

            leftItem = left / amount;
        }

        public static void CalculateLvUp(int startLv, int startExp, int expAmount,
            out int lv, out int exp, out int totalExp, out int remainAmount)
        {
            lv = startLv;
            exp = startExp;
            totalExp = 0;
            remainAmount = expAmount;

            var db = _db.Levels;
            int maxLv = db.MaxLv;

            while (db.TryFind(lv, out var entity) && remainAmount > 0)
            {
                int forNext = entity.exp;
                int requireExp = forNext - exp;
                int add = Mathf.Min(requireExp, remainAmount);
                exp += add;
                totalExp += add;
                remainAmount -= add;

                int remain = exp - forNext;
                if (remain >= 0)
                {
                    ++lv;
                    exp = lv >= maxLv ? 0 : remain;
                }
            }
        }

        public static void CalculateUpgrade(int unitId, int startRate, ItemEntity item, int quantity,
            out int rate, out int leftItem, out int price)
        {
            rate = startRate;
            leftItem = quantity;
            price = 0;

            if (!_db.Units.TryFind(unitId, out var unitEntity) ||
                unitEntity.soulItemId != item.Id)
                return;

            var db = _db.Rates;

            while (leftItem > 0 && db.TryFind(rate, out var entity))
            {
                int requireSoul = entity.soul;
                if (leftItem < requireSoul)
                    break;

                leftItem -= requireSoul;
                price += entity.gold;
                ++rate;
            }
        }
    }
}
