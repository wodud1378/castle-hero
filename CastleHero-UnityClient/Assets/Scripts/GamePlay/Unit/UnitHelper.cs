using System.Collections.Generic;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;
using CastleHero.GamePlay.Unit;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.GamePlay.Unit.Finding;
using CastleHero.GamePlay.Unit.Skill.Components.Factory;
using UnityEngine;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.Utility
{
    public class IdDescendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            return x.id.CompareTo(y.id);
        }
    }

    public class LvDescendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int lvComparison = y.lv.CompareTo(x.lv);
            if (lvComparison == 0)
            {
                return x.id.CompareTo(y.id);
            }

            return lvComparison;
        }
    }

    public class LvAscendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int lvComparison = x.lv.CompareTo(y.lv);
            if (lvComparison == 0)
            {
                return x.id.CompareTo(y.id);
            }

            return lvComparison;
        }
    }

    public class RateDescendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int rateComparison = y.rate.CompareTo(x.rate);
            if (rateComparison == 0)
            {
                return x.id.CompareTo(y.id);
            }

            return rateComparison;
        }
    }

    public class RateAscendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int rateComparison = x.rate.CompareTo(y.rate);
            if (rateComparison == 0)
            {
                return x.id.CompareTo(y.id);
            }

            return rateComparison;
        }
    }

    public static class UnitHelper
    {
        private static IDBProvider _db;

        static UnitHelper()
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

            var levels = _db.Levels;
            int maxLv = levels.MaxLv;
            while (levels.TryFind(lv, out var entity) && left > 0)
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

            var levels = _db.Levels;
            int maxLv = levels.MaxLv;

            while (levels.TryFind(lv, out var entity) && remainAmount > 0)
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
            out int rate,
            out int leftItem,
            out int price)
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

        public static void AdditionalStatus(this UnitBalanceEntity balanceData, int lv, int rate, out Dictionary<Status.Type, float> stats, out int skillLv)
        {
            skillLv = 1;
            stats = null;
            stats = new Dictionary<Status.Type, float>
            {
                { Status.Type.Hp, balanceData.hp * lv },
                { Status.Type.Atk, balanceData.atk * lv },
                { Status.Type.Critical, balanceData.critical * lv },
                { Status.Type.CriticalAtk, balanceData.criticalAtk * lv },
                { Status.Type.AtkSpeed, balanceData.atkSpeed * lv },
                { Status.Type.MoveSpeed, balanceData.speed * lv },
                { Status.Type.AtkRange, balanceData.atkRange * lv },
                { Status.Type.MoveRange, balanceData.moveRange * lv }
            };

            if (balanceData.rateOptions == null || balanceData.rateValues == null)
                return;

            int rateBonusLength = balanceData.rateOptions.Length;
            int rateIndex = Mathf.Clamp(rate, 0, rateBonusLength) - 1;
            if (rateIndex == -1)
                return;

            for (int i = 0; i < rateIndex; ++i)
            {
                var options = balanceData.rateOptions[i];
                var values = balanceData.rateValues[i];
                int length = options.Length;
                for (int j = 0; j < length; ++j)
                {
                    Status.Type type;
                    switch (options[j])
                    {
                        case 0:
                            skillLv = skillLv > values[j] ? skillLv : (int)values[j];
                            continue;
                        case 1:
                            type = Status.Type.Atk;
                            break;
                        case 2:
                            type = Status.Type.Hp;
                            break;
                        case 3:
                            type = Status.Type.AtkSpeed;
                            break;
                        case 4:
                            type = Status.Type.MoveSpeed;
                            break;
                        case 5:
                            type = Status.Type.Critical;
                            break;
                        case 6:
                            type = Status.Type.CriticalAtk;
                            break;
                        default:
                            continue;
                    }

                    if (!stats.TryAdd(type, values[j]))
                        stats[type] += values[j];
                }
            }
        }

        public static void SetFilter(this IDetection detection, UnitActor owner, Targeting targeting)
        {
            switch (targeting)
            {
                case Targeting.Alley:
                    detection.Filter = owner.UnitState.AlleyLayerMask;
                    break;
                case Targeting.Enemy:
                    detection.Filter = owner.UnitState.EnemyLayerMask;
                    break;
                case Targeting.Both:
                    detection.Filter = owner.UnitState.AlleyLayerMask | owner.UnitState.EnemyLayerMask;
                    break;
            }
        }

        public static LayerMask EnemyLayerMask(int id, int atkType)
        {
            LayerMask layerMask = default;

            string tag = EnemyTag(id);
            int groundUnit = 1 << DefTypeToLayer(tag, 1);
            int flightUnit = 1 << DefTypeToLayer(tag, 2);
            switch (atkType)
            {
                case 0:
                    layerMask = groundUnit | flightUnit;
                    break;
                case 1:
                    layerMask = groundUnit;
                    break;
                case 2:
                    layerMask = flightUnit;
                    break;
            }

            return layerMask;
        }

        public static LayerMask AlleyLayerMask(int id)
        {
            string tag = AlleyTag(id);

            return (1 << DefTypeToLayer(tag, 1))
                   | (1 << DefTypeToLayer(tag, 2));
        }

        public static void InitAlley(this UnitActor unit, int defLayer)
        {
            var alleyTag = AlleyTag(unit.Id);
            var alleyLayer = DefTypeToLayer(alleyTag, defLayer);
            var go = unit.gameObject;

            go.tag = alleyTag;
            go.layer = alleyLayer;
        }

        public static bool IsAlley(this UnitActor a, UnitActor b) => a.gameObject.CompareTag(b.tag);

        public static string AlleyTag(this int id) => id.ToString().StartsWith("1") ? "Character" : "Monster";

        public static string EnemyTag(this int id) => id.ToString().StartsWith("1") ? "Monster" : "Character";

        private static int DefTypeToLayer(string tag, int defType)
        {
            string type = defType switch
            {
                1 => "Ground",
                2 => "Flight",
                _ => string.Empty
            };

            return LayerMask.NameToLayer($"{type}{tag}");
        }

        public static UnitActor SelectTarget(Finder finder, UnitActor owner, UnitActor currentTarget, float range)
        {
            var overriden = finder.Override;
            if (overriden.IsValid())
                return overriden;

            if (currentTarget.IsValid())
            {
                float distance = (currentTarget.Position - owner.Position).sqrMagnitude;
                float sqrRange = range * range;

                if (distance <= sqrRange)
                    return currentTarget;
            }

            finder.detection.SetRange(range, range);
            return !finder.Update(owner.Position)
                ? null
                : finder.Found.Count > 0
                    ? finder.Found[0]
                    : null;
        }

        public static bool IsValid(this UnitActor unit)
        {
            if (unit == null)
                return false;

            return unit.State.Value is > UnitState.States.Prepare and < UnitState.States.Dead;
        }
    }
}
