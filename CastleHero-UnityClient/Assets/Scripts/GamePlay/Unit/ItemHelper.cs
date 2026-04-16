using System.Collections.Generic;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;
using CastleHero.GamePlay.Unit;
using CastleHero.GamePlay.Unit.Components;

namespace CastleHero.Utility
{
    /// <summary>
    /// Equipment-specific item helpers that depend on Unit component types (Status, Elemental).
    /// Domain-level item helpers are in Domain/Utility/ItemHelper.cs.
    /// </summary>
    public static class EquipmentHelper
    {
        public static Dictionary<Status.Type, float> Total(this IEnumerable<EquipItem> equipments, ref Elemental elemental)
        {
            var dic = new Dictionary<Status.Type, float>();
            elemental ??= new Elemental
            {
                atkType = ElementalType.None,
                defType = ElementalType.None
            };

            if (equipments != null)
            {
                foreach (var equipment in equipments)
                {
                    switch ((EquipmentSlot)equipment.slot)
                    {
                        case EquipmentSlot.Armor:
                            elemental.defType = (ElementalType)equipment.element.type;
                            elemental.defLv = equipment.element.lv;
                            break;
                        case EquipmentSlot.Weapon:
                            elemental.atkType = (ElementalType)equipment.element.type;
                            elemental.atkLv = equipment.element.lv;
                            break;
                    }

                    var main = equipment.main;
                    var type = (Status.Type)main.type;
                    var value = main.value;
                    if (value != 0f)
                    {
                        if (!dic.TryAdd(type, value))
                            dic[type] += value;
                    }

                    foreach (var stat in equipment.sub)
                    {
                        type = (Status.Type)stat.type;
                        value = stat.value;

                        if (value == 0f)
                            continue;

                        if (!dic.TryAdd(type, value))
                            dic[type] += value;
                    }
                }
            }

            return dic;
        }
    }
}
