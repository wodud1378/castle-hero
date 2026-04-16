using System;

namespace CastleHero.Utility
{
    public static class EnumHelper
    {
        public static unsafe int CastToInt<T>(this T val) where T : unmanaged, Enum => *(int*)&val;

        public static bool HasFlagUnSafe<T>(this T it, T value) where T : unmanaged, Enum
        {
            int castedIt = CastToInt(it);
            int castedVal = CastToInt(value);

            return (castedIt & castedVal) == castedVal;
        }
    }
}