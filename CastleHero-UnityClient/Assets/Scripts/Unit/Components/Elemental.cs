using System.Collections.Generic;

namespace RGLabs.Unit.Components
{
    public class Elemental
    {
        private static readonly Dictionary<Type, Dictionary<Type, float>> Map = new()
        {
            {
                Type.None, new Dictionary<Type, float>
                {
                    { Type.None, 1f },
                    { Type.Ground, 0.8f },
                    { Type.Fire, 0.8f },
                    { Type.Wind, 0.8f },
                    { Type.Water, 0.8f },
                }
            },
            {
                Type.Ground, new Dictionary<Type, float>
                {
                    { Type.None, 1.2f },
                    { Type.Ground, 1f },
                    { Type.Fire, 1f },
                    { Type.Wind, 0.8f },
                    { Type.Water, 1f },
                }
            },
            {
                Type.Fire, new Dictionary<Type, float>
                {
                    { Type.None, 1.2f },
                    { Type.Ground, 0.8f },
                    { Type.Fire, 0.8f },
                    { Type.Wind, 0.8f },
                    { Type.Water, 0.8f },
                }
            },
            {
                Type.Wind, new Dictionary<Type, float>
                {
                    { Type.None, 1.2f },
                    { Type.Ground, 0.8f },
                    { Type.Fire, 0.8f },
                    { Type.Wind, 0.8f },
                    { Type.Water, 0.8f },
                }
            },
            {
                Type.Water, new Dictionary<Type, float>
                {
                    { Type.None, 1.2f },
                    { Type.Ground, 0.8f },
                    { Type.Fire, 0.8f },
                    { Type.Wind, 0.8f },
                    { Type.Water, 0.8f },
                }
            }
        };

        public enum Type
        {
            None = 0,
            Ground,
            Fire,
            Wind,
            Water,
        }

        public Type atkType;
        public Type defType;

        public static float AtkMultiplier(Elemental elemental) => AtkMultiplier(elemental.atkType, elemental.defType);
        
        private static float AtkMultiplier(Type atk, Type def)
        {
            if (!Map.TryGetValue(atk, out var subMap))
                return 1f;

            return subMap.GetValueOrDefault(def, 1f);
        }
    }
}