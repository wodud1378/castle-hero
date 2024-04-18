using System.Collections;
using RGLabs.InGame.Behaviours.Unit;
using UnityEngine;

namespace RGLabs.InGame.Utility
{
    public static class Utility
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

        public static bool IsFar(this Vector2 from, Vector2 to, float threshold) => !from.IsNear(to, threshold);

        public static bool IsNear(this Vector2 from, Vector2 to, float threshold)
        {
            float thresholdPow = Mathf.Pow(threshold, 2);
            float distance = from.DistanceTo(to);
            if (Mathf.Approximately(distance, thresholdPow))
                return true;

            return distance < thresholdPow;
        }

        public static T[] Shuffle<T>(this T[] array) => array.Shuffle(0, array.Length);

        public static T[] Shuffle<T>(this T[] array, int length) => array.Shuffle(0, length);
        
        public static T[] Shuffle<T>(this T[] array, int index, int length)
        {
            for (int i = index; i < length; ++i)
            {
                var a = Random.Range(0, array.Length);
                var b = Random.Range(0, array.Length);

                (array[a], array[b]) = (array[b], array[a]);
            }

            return array;
        }

        public static bool IsValid(this UnitBehaviour unit)
        {
            if (unit == null)
                return false;

            return unit.State is > UnitBehaviour.States.Prepare and < UnitBehaviour.States.Dead;
        }

        public static bool IsOutOfRange(this int index, IList target) => index < 0 || target.Count <= index;
    }
}