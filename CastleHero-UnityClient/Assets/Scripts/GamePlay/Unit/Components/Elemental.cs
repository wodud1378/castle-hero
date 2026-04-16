using System.Collections.Generic;
using CastleHero.Data;
using CastleHero.Data.Model;
using UnityEngine;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.GamePlay.Unit.Components
{
    // ElementalType enum is defined in CastleHero.Data.Model.ElementalType (Domain layer).
    // This alias keeps existing code in this assembly working without changes.
    using Type = ElementalType;

    public class Elemental
    {
        private static readonly Dictionary<Type, (Type forward, Type reverse)> Compatibility = new()
        {
            { Type.Earth, (Type.Water, Type.Wind) },
            { Type.Fire, (Type.Wind, Type.Water) },
            { Type.Water, (Type.Fire, Type.Earth) },
            { Type.Wind, (Type.Earth, Type.Fire) }
        };

        public int atkLv;
        public int defLv;
        public Type atkType;
        public Type defType;

        public static float AtkMultiplier(Elemental atk, Elemental def)
        {
            if (atk.atkType == Type.None)
            {
                return def.defType != Type.None
                    ? ServiceLocator.Get<IDBProvider>().Elements[0].reverse
                    : 1f;
            }

            var entity = ServiceLocator.Get<IDBProvider>().Elements[Mathf.Clamp(def.atkLv - atk.defLv, 0, 2)];
            var data = Compatibility[atk.atkType];
            return def.defType == data.forward
                ? entity.forward
                : def.defType == data.reverse
                    ? entity.reverse
                    : 1f;
        }
    }
}
