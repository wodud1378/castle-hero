using System.Collections;
using RGLabs.InGame.Behaviours.Unit;
using UnityEngine;

namespace RGLabs.InGame.Utility
{
    public static class Utility
    {
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

        public static T[] Shuffle<T>(this T[] array)
        {
            int random1, random2;
            T temp;

            for (int i = 0; i < array.Length; ++i)
            {
                random1 = Random.Range(0, array.Length);
                random2 = Random.Range(0, array.Length);

                temp = array[random1];
                array[random1] = array[random2];
                array[random2] = temp;
            }

            return array;
        }

        public static bool IsValid(this UnitBehaviour unit)
        {
            if (unit == null)
                return false;

            return unit.State != UnitBehaviour.States.Dead && unit.gameObject.activeSelf;
        }

        public static bool IsOutOfRange(this int index, IList target) => index < 0 || target.Count <= index;
    }
}