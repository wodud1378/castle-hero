using System.Collections.Generic;
using RGLabs.Data;
using UnityEngine;

namespace RGLabs.Unit.Components
{
    public class Elemental
    {
        public enum Type
        {
            None = 0,
            Earth,
            Fire,
            Wind,
            Water,
        }

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
                    ? Storage.db.elements[0].reverse
                    : 1f;
            }
            
            var entity = Storage.db.elements[Mathf.Clamp(def.atkLv - atk.defLv, 0, 2)];
            var data = Compatibility[atk.atkType];
            return def.defType == data.forward
                ? entity.forward
                : def.defType == data.reverse
                    ? entity.reverse
                    : 1f;
        }
    }
}