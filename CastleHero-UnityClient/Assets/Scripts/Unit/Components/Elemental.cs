using System.Collections.Generic;

namespace RGLabs.Unit.Components
{
    public class Elemental
    {
        public enum Type
        {
            None = 0,
            Ground,
            Fire,
            Wind,
            Water,
        }
        
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
                    { Type.None, BaseMultiplier },
                    { Type.Ground, 1f },
                    { Type.Fire, 1f },
                    { Type.Wind, 0.8f },
                    { Type.Water, 1f },
                }
            },
            {
                Type.Fire, new Dictionary<Type, float>
                {
                    { Type.None, BaseMultiplier },
                    { Type.Ground, 0.8f },
                    { Type.Fire, 0.8f },
                    { Type.Wind, 0.8f },
                    { Type.Water, 0.8f },
                }
            },
            {
                Type.Wind, new Dictionary<Type, float>
                {
                    { Type.None, BaseMultiplier },
                    { Type.Ground, 0.8f },
                    { Type.Fire, 0.8f },
                    { Type.Wind, 0.8f },
                    { Type.Water, 0.8f },
                }
            },
            {
                Type.Water, new Dictionary<Type, float>
                {
                    { Type.None, BaseMultiplier },
                    { Type.Ground, 0.8f },
                    { Type.Fire, 0.8f },
                    { Type.Wind, 0.8f },
                    { Type.Water, 0.8f },
                }
            }
        };

        private const float BaseMultiplier = 1.1f;
        private const float LevelMultiplier = 0.05f;
        
        public int level = 1;
        public Type atkType;
        public Type defType;

        public static float AtkMultiplier(Elemental elemental)
        {
            if (!Map.TryGetValue(elemental.atkType, out var subMap))
                return 1f;

            return subMap.GetValueOrDefault(elemental.defType, 1f) + (elemental.level * LevelMultiplier);
        }
    }
}