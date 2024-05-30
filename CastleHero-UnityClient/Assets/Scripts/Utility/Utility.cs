using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Unit.Finding;
using RGLabs.Unit.Skill.Components.Factory;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RGLabs.Utility
{
    public static class ItemHelder
    {
        public static ItemTypeCode ItemType(this int id) => (ItemTypeCode)(id / 10000);
    }
    
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
    
    public static class AddressableHelper
    {
        public static async UniTask<AsyncOperationHandle<T>> Handle<T>(this string key)
        {
            var handle = Addressables.LoadAssetAsync<T>(key);
            await handle.ToUniTask();
            return handle;
        }

        public static void Release<T>(this AsyncOperationHandle<T> handle)
        {
            if (!handle.IsValid())
                return;

            Addressables.Release(handle);
        }

        public static void Release(this AsyncOperationHandle handle)
        {
            if (!handle.IsValid())
                return;

            Addressables.Release(handle);
        }
    }

    public static class UnitHelper
    {
        public static void SetFilter(this IDetection detection, UnitBehaviour owner, Targeting targeting)
        {
            switch (targeting)
            {
                case Targeting.Alley:
                    detection.Filter = owner.Core.alleyLayerMask;
                    break;
                case Targeting.Enemy:
                    detection.Filter = owner.Core.enemyLayerMask;
                    break;
                case Targeting.Both:
                    detection.Filter = owner.Core.alleyLayerMask | owner.Core.enemyLayerMask;
                    break;
            }
        }
        
        public static LayerMask EnemyLayerMask(int id, int atkType)
        {
            LayerMask layerMask = default;

            string tag = EnemyTag(id);
            int groundUnit = 1 << DefTypeToLayer(tag, 1);
            int flightUnit = 1 << DefTypeToLayer(tag, 2);
            switch (atkType)
            {
                case 0:
                    layerMask = groundUnit | flightUnit;
                    break;
                case 1:
                    layerMask = groundUnit;
                    break;
                case 2:
                    layerMask = flightUnit;
                    break;
            }

            return layerMask;
        }

        public static LayerMask AlleyLayerMask(int id)
        {
            string tag = AlleyTag(id);

            return (1 << DefTypeToLayer(tag, 1))
                   | (1 << DefTypeToLayer(tag, 2));
        }

        public static void InitAlley(this UnitBehaviour unit, int defLayer)
        {
            var alleyTag = AlleyTag(unit.Id);
            var alleyLayer = DefTypeToLayer(alleyTag, defLayer);
            var go = unit.gameObject;

            go.tag = alleyTag;
            go.layer = alleyLayer;
        }

        public static bool IsAlley(this UnitBehaviour a, UnitBehaviour b) => a.gameObject.CompareTag(b.tag);

        public static string AlleyTag(this int id) => id.ToString().StartsWith("1") ? "Character" : "Monster";

        public static string EnemyTag(this int id) => id.ToString().StartsWith("1") ? "Monster" : "Character";

        private static int DefTypeToLayer(string tag, int defType)
        {
            string type = defType switch
            {
                1 => "Ground",
                2 => "Flight",
                _ => string.Empty
            };

            return LayerMask.NameToLayer($"{type}{tag}");
        }

        public static bool IsValid(this UnitBehaviour unit)
        {
            if (unit == null)
                return false;

            return unit.state.Value is > UnitCore.States.Prepare and < UnitCore.States.Dead;
        }
    }

    public static class ObjectHelper
    {
        public static void ToUILayer(this GameObject obj) => obj.ToLayer("UI");

        public static void ToLayer(this GameObject obj, string layer)
        {
            if (!obj.TryGetComponent(out SortingGroup sortingGroup))
                return;

            int id = SortingLayer.NameToID(layer);
            sortingGroup.sortingLayerID = id;
        }
    }

    public static class RxHelper
    {
        public static IObservable<ReactiveCollection<T>> ChangeAsObservable<T>(this ReactiveCollection<T> collection)
        {
            return Observable.Create<ReactiveCollection<T>>(observer =>
            {
                var disposableAdd = collection.ObserveAdd()
                    .Subscribe(ev => observer.OnNext(collection));

                var disposableRemove = collection.ObserveRemove()
                    .Subscribe(ev => observer.OnNext(collection));

                return Disposable.Create(() =>
                {
                    disposableAdd.Dispose();
                    disposableRemove.Dispose();
                });
            });
        }
        
        public static void SubscribeMessage<T>(this MonoBehaviour behaviour, Action<T> onReceive)
        {
            MessageBroker.Default
                .Receive<T>()
                .Subscribe(onReceive)
                .AddTo(behaviour);
        }

        public static void Publish<T>(this T data) => MessageBroker.Default.Publish(data);

        public static void SubscribeButton(this MonoBehaviour behaviour, Button button, Action onClick,
            float clickThreshold = 0.25f)
        {
            button
                .OnClickAsObservable()
                .ThrottleFirst(TimeSpan.FromSeconds(clickThreshold))
                .Subscribe(_ => onClick.Invoke())
                .AddTo(behaviour);
        }
    }

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

        public static bool IsValidIndex(this int index, params IList[] listCollection) => listCollection.All(list => index.IsValidIndex(list));

        public static bool IsValidIndex(this int index, IList target)
        {
            if (target == null)
                return false;
            
            return index >= 0 && target.Count > index;
        }
    }
}