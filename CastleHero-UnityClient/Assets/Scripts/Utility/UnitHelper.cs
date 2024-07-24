using System.Collections.Generic;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Unit;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Unit.Finding;
using RGLabs.Unit.Skill.Components.Factory;
using UnityEngine;

namespace RGLabs.Utility
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
        
        public static void SetFilter(this IDetection detection, UnitBehaviour owner, Targeting targeting)
        {
            switch (targeting)
            {
                case Targeting.Alley:
                    detection.Filter = owner.Core.alleyLayerMask;
                    break;
                case Targeting.Enemy:
                    detection.Filter = owner.Core.enemyLayerMask;
                    break;
                case Targeting.Both:
                    detection.Filter = owner.Core.alleyLayerMask | owner.Core.enemyLayerMask;
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

        public static void InitAlley(this UnitBehaviour unit, int defLayer)
        {
            var alleyTag = AlleyTag(unit.Id);
            var alleyLayer = DefTypeToLayer(alleyTag, defLayer);
            var go = unit.gameObject;

            go.tag = alleyTag;
            go.layer = alleyLayer;
        }

        public static bool IsAlley(this UnitBehaviour a, UnitBehaviour b) => a.gameObject.CompareTag(b.tag);

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

        public static bool IsValid(this UnitBehaviour unit)
        {
            if (unit == null)
                return false;

            return unit.state.Value is > UnitCore.States.Prepare and < UnitCore.States.Dead;
        }
    }
}