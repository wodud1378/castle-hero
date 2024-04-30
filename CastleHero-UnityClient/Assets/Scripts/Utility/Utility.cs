using System.Collections;
using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Factory;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RGLabs.Utility
{
    public static class AddressableHelper
    {
        public static async UniTask<AsyncOperationHandle<Sprite>> LoadImage(this Image image, string key)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(key);
            var sprite = await handle.ToUniTask();

            image.sprite = sprite;
            return handle;
        }

        public static async UniTask<AsyncOperationHandle<T>> Handle<T>(this string key)
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            await handle.ToUniTask();
            return handle;
        }

        public static void Release<T>(this AsyncOperationHandle<T> handle) => Addressables.Release(handle);
    }

    public static class Utility
    {
        public static void ToUILayer(this GameObject obj) => obj.ToLayer("UI");

        public static void ToLayer(this GameObject obj, string layer)
        {
            if (!obj.TryGetComponent(out SortingGroup sortingGroup))
                return;

            int id = SortingLayer.NameToID(layer);
            sortingGroup.sortingLayerID = id;
        }


        public static async UniTask<T> Create<T>(this UnitEntity data, Vector2 position, IUnitFactory factory)
            where T : UnitBehaviour
        {
            return await factory.Create<T>(data, position);
        }

        public static void Publish<T>(this T data)
        {
            MessageBroker.Default.Publish(data);
        }

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

            return unit.state.Value is > UnitBehaviour.States.Prepare and < UnitBehaviour.States.Dead;
        }

        public static bool IsValidIndex(this int index, IList target) => index >= 0 && target.Count > index;
    }
}