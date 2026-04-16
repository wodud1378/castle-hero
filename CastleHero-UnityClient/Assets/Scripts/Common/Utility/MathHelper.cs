using UnityEngine;

namespace CastleHero.Utility
{
    public static class MathHelper
    {
        public static Vector2 ToVector(this float degree)
        {
            float rad = degree * Mathf.Deg2Rad;

            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        public static float ToFloat(this Vector2 direction)
        {
            float radian = Mathf.Atan2(direction.x, direction.y);
            return radian * Mathf.Rad2Deg;
        }

        public static Vector2 Rotate(this Vector2 point, Vector2 pivot, float angle)
        {
            Vector2 dir = point - pivot;
            dir = Quaternion.Euler(0f, 0f, angle) * dir;
            point = dir + pivot;
            return point;
        }

        public static float DistanceTo(this Vector2 from, Vector2 to) => (to - from).sqrMagnitude;
        
        public static bool IsNear(this Vector2 from, Vector2 to)
        {
            float distance = from.DistanceTo(to);
            if (distance > 0.015f)
                return false;

            return true;
        }
    }
}