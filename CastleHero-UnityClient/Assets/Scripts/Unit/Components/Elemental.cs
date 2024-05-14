using System.Collections.Generic;

namespace RGLabs.Unit.Components
{
    public class Elemental
    {
        //private static readonly Dictionary<Type, Type>
        
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

        // public static float Convert(Type atk, Type def)
        // {
        //     
        // }
    }
}