using System.Collections;
using System.Linq;
using UnityEngine;

namespace CastleHero.Utility
{
    public static class CollectionHelper
    {
        public static T[] Shuffle<T>(this T[] array) => array.Shuffle(0, array.Length);

        public static T[] Shuffle<T>(this T[] array, int length) => array.Shuffle(0, length);

        public static T[] Shuffle<T>(this T[] array, int index, int length)
        {
            for (int i = index; i < length; ++i)
            {
                var a = Random.Range(0, length);
                var b = Random.Range(0, length);

                (array[a], array[b]) = (array[b], array[a]);
            }

            return array;
        }

        public static bool IsValidIndex(this int index, params IList[] listCollection) =>
            listCollection.All(list => IsValidIndex(index, (IList)list));

        public static bool IsValidIndex(this int index, IList target)
        {
            if (target == null)
                return false;

            return index >= 0 && target.Count > index;
        }
    }
}