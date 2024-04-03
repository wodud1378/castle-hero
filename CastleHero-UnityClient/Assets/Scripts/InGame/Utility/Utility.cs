using UnityEngine;

namespace RGLabs.InGame.Utility
{
    public static class Utility
    {
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
    }
}