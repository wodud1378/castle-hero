using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RGLabs.Common.Behaviours;
using RGLabs.Data.DB;
using RGLabs.Data.Load;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Unit;
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
    public class PrefabPathAttribute : Attribute
    {
        public string Path { get; }

        public PrefabPathAttribute(string path) => Path = path;
    }

    public static class PrefabPathCache
    {
        private static Dictionary<Type, string> _cache;

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            var assembly = Assembly.Load("Assembly-CSharp");
            _cache = assembly.GetTypes()
                .Where(x => x.IsClass && x.GetCustomAttribute<PrefabPathAttribute>() != null)
                .ToDictionary(x => x, y => y.GetCustomAttribute<PrefabPathAttribute>().Path);
        }

        public static string Load(Type type) => _cache.GetValueOrDefault(type);
    }

    public static class ReflectionHelper
    {
        public static List<IDataField> GetDataFields(this Type type)
        {
            return type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(ToInterface)
                .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                    .Select(ToInterface))
                .Where(x => x != null)
                .ToList();
        }
        
        public static bool TryParse(this string value, Type type, out object result)
        {
            bool success = false;
            if (type.IsEnum)
            {
                if (int.TryParse(value, out int enumVal))
                {
                    result = Enum.ToObject(type, enumVal);
                    return true;
                }

                result = null;
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    success = bool.TryParse(value, out var boolean);
                    result = success && boolean;
                    break;
                case TypeCode.Int32:
                    success = int.TryParse(value, out var int32);
                    result = success ? int32 : -1;
                    break;
                case TypeCode.Int64:
                    success = long.TryParse(value, out var int64);
                    result = success ? int64 : -1;
                    break;
                case TypeCode.Single:
                    success = float.TryParse(value, out var single);
                    result = success ? single : -1f;
                    break;
                case TypeCode.Double:
                    success = double.TryParse(value, out var @double);
                    result = success ? @double : -1;
                    break;
                case TypeCode.String:
                    success = true;
                    result = value;
                    break;
                default:
                    result = null;
                    break;
            }

            return success;
        }
        
        public static Type GetEntityType(this Type type)
        {
            type = type.BaseType;
            if (type == null)
                return null;

            if (!type.IsGenericType)
                return null;

            return type.GenericTypeArguments[0];
        }
        
        private static IDataField ToInterface(MemberInfo info)
        {
            var attribute = GetDataFieldAttribute(info);
            if (attribute == null)
                return null;

            switch (info)
            {
                case FieldInfo fieldInfo:
                    return new DataField(fieldInfo, attribute);
                case PropertyInfo propertyInfo:
                    return new PropertyDataField(propertyInfo, attribute);
            }

            return null;
        }
        
        private static DataFieldAttribute GetDataFieldAttribute(MemberInfo memberInfo)
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(DataFieldAttribute), false);
            if (attributes.Length == 0)
                return null;

            return attributes.Cast<DataFieldAttribute>().First();
        }
    }

    public static class ItemHelder
    {
        public static Dictionary<Status.Type, float> Total(this IEnumerable<EquipItem> equipments)
        {
            var dic = new Dictionary<Status.Type, float>();
            if (equipments != null)
            {
                foreach (var equipment in equipments)
                {
                    var main = equipment.main;
                    var type = (Status.Type)main.type;
                    var value = main.value;
                    if (value != 0f)
                    {
                        if (!dic.TryAdd(type, value))
                        {
                            dic[type] += value;
                        }   
                    }
                    
                    foreach (var stat in equipment.sub)
                    {
                        type = (Status.Type)stat.type;
                        value = stat.value;
                        
                        if (value == 0f)
                            continue;
                        
                        if (!dic.TryAdd(type, value))
                            dic[type] += value;
                    }
                }
            }

            return dic;
        }
    }

    public static class StringHelper
    {
        private const string ColoredStringTag = "<color={0}>{1}</color>";
        
        private static readonly Color Positive = Color.green;
        private static readonly Color Negative = Color.red;

        public static string CurrencyText(this int value) => value.ToString("N0");
        
        public static string WithPositiveColor(this string text) => text.WithColor(Positive);
        public static string WithNegativeColor(this string text) => text.WithColor(Negative);
        
        public static string WithColor(this string text, Color color) => string.Format(ColoredStringTag, color.Hex(), text);

        private static string Hex(this Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            int a = Mathf.RoundToInt(color.a * 255);
            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        }
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
        public static async UniTask<T> Instantiate<T>(this AssetReference reference, Transform transform,
            CancellationToken ct = default)
        {
            var task = reference.InstantiateAsync(transform)
                .ToUniTask(cancellationToken: ct)
                .SuppressCancellationThrow();

            var obj = await task;
            return obj.Result == null ? default : obj.Result.GetComponent<T>();
        }

        public static async UniTask<T> Instantiate<T>(this string path, Transform transform,
            CancellationToken ct = default)
        {
            var task = Addressables.InstantiateAsync(path, transform)
                .ToUniTask(cancellationToken: ct)
                .SuppressCancellationThrow();

            var obj = await task;
            return obj.Result == null ? default : obj.Result.GetComponent<T>();
        }

        public static async UniTask<T> Load<T>(this string path, CancellationToken cancellationToken = default)
        {
            var handle = Addressables.LoadAssetAsync<T>(path);
            await handle
                .ToUniTask(cancellationToken: cancellationToken)
                .SuppressCancellationThrow();

            return handle.Result;
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
        public static void AdditionalStatus(this UnitBalanceEntity balanceData, int lv, int rate, out Dictionary<Status.Type, float> stats, out int skillLv)
        {
            skillLv = 1;
            stats = null;
            stats = new Dictionary<Status.Type, float>
            {
                { Status.Type.Hp, balanceData.hp * lv },
                { Status.Type.Atk, balanceData.atk * lv },
                { Status.Type.Critical, balanceData.critical * lv },
                { Status.Type.CriticalAtk, balanceData.criticalAtk * lv },
                { Status.Type.AtkSpeed, balanceData.atkSpeed * lv },
                { Status.Type.MoveSpeed, balanceData.speed * lv },
                { Status.Type.AtkRange, balanceData.atkRange * lv },
                { Status.Type.MoveRange, balanceData.moveRange * lv }
            };
            
            if (balanceData.rateOptions == null || balanceData.rateValues == null)
                return;

            int rateBonusLength = balanceData.rateOptions.Length;
            int rateIndex = Mathf.Clamp(rate, 0, rateBonusLength) - 1;
            if (rateIndex == -1)
                return;

            for (int i = 0; i < rateIndex; ++i)
            {
                var options = balanceData.rateOptions[i];
                var values = balanceData.rateValues[i];
                int length = options.Length;
                for (int j = 0; j < length; ++j)
                {
                    Status.Type type;
                    switch (options[j])
                    {
                        case 0:
                            skillLv = skillLv > values[j] ? skillLv : (int)values[j];
                            continue;
                        case 1:
                            type = Status.Type.Atk;
                            break;
                        case 2:
                            type = Status.Type.Hp;
                            break;
                        case 3:
                            type = Status.Type.AtkSpeed;
                            break;
                        case 4:
                            type = Status.Type.MoveSpeed;
                            break;
                        case 5:
                            type = Status.Type.Critical;
                            break;
                        case 6:
                            type = Status.Type.CriticalAtk;
                            break;
                        default:
                            continue;
                    }

                    if (!stats.TryAdd(type, values[j]))
                        stats[type] += values[j];
                }
            }
        }
        
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

    public class IdDescendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            return x.id.CompareTo(y.id);
        }
    }

    public class LvDescendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int lvComparison = y.lv.CompareTo(x.lv);
            if (lvComparison == 0)
            {
                return x.id.CompareTo(y.id);
            }

            return lvComparison;
        }
    }

    public class LvAscendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int lvComparison = x.lv.CompareTo(y.lv);
            if (lvComparison == 0)
            {
                return x.id.CompareTo(y.id);
            }

            return lvComparison;
        }
    }

    public class RateDescendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int rateComparison = y.rate.CompareTo(x.rate);
            if (rateComparison == 0)
            {
                return x.id.CompareTo(y.id);
            }

            return rateComparison;
        }
    }

    public class RateAscendingComparer : IComparer<UnitInfo>
    {
        public int Compare(UnitInfo x, UnitInfo y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            int rateComparison = x.rate.CompareTo(y.rate);
            if (rateComparison == 0)
            {
                return x.id.CompareTo(y.id);
            }

            return rateComparison;
        }
    }

    public static class ObjectHelper
    {
        public static void ToPreviewLayer(this GameObject obj) => obj.ToLayer("UnitPreview");

        public static void ToLayer(this GameObject obj, string layer) => obj.ToLayer(SortingLayer.NameToID(layer));

        public static void ToLayer(this GameObject obj, int layer)
        {
            if (!obj.TryGetComponent(out SortingGroup sortingGroup))
                return;

            sortingGroup.sortingLayerID = layer;
        }

        public static int GetLayer(this GameObject obj)
        {
            if (!obj.TryGetComponent(out SortingGroup sortingGroup))
                return 0;

            return sortingGroup.sortingLayerID;
        }

        public static Vector3 ScreenToWorld(this Vector2 screenPoint)
        {
            var camera = Camera.main;
            if (camera == null)
                return default;

            var position = camera.ScreenToWorldPoint(screenPoint);
            position.z = 0f;
            return position;
        }
    }

    public static class TaskHelper
    {
        public static UniTask OnAnimationEnd(Animator animator, int hash, Action onEnd)
        {
            animator.SetTrigger(hash);
            
            return Observable
                .EveryUpdate()
                .Where(_ =>
                {
                    var state = animator.GetCurrentAnimatorStateInfo(0);
                    return state.shortNameHash == hash && state.normalizedTime >= 1f;
                })
                .First()
                .ToUniTask()
                .ContinueWith(_ => onEnd?.Invoke());
        }
    }

    public static class RxHelper
    {
        public static IObservable<ReactiveCollection<T>> ChangeAsObservable<T>(this ReactiveCollection<T> collection)
        {
            return Observable.Create<ReactiveCollection<T>>(observer =>
            {
                var clear = collection.ObserveReset()
                    .Subscribe(_ => observer.OnNext(collection));

                var add = collection.ObserveAdd()
                    .Subscribe(_ => observer.OnNext(collection));

                var remove = collection.ObserveRemove()
                    .Subscribe(_ => observer.OnNext(collection));

                return Disposable.Create(() =>
                {
                    clear.Dispose();
                    add.Dispose();
                    remove.Dispose();
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

        public static bool IsValidIndex(this int index, params IList[] listCollection) =>
            listCollection.All(list => index.IsValidIndex(list));

        public static bool IsValidIndex(this int index, IList target)
        {
            if (target == null)
                return false;

            return index >= 0 && target.Count > index;
        }
    }

    public static class NetworkHelper
    {
        private static readonly JsonSerializerSettings DefaultSetting = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };
        
        public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj, DefaultSetting);

        public static T Cast<T>(this JsonData data)
        {
            var str = data.ToJson();
            if (str.StartsWith("\""))
                str = str.Remove(0, 1);
            if (str.EndsWith("\""))
                str = str.Remove(str.Length - 1, 1);
            
            str = str
                .Replace("BackendFunction", "Assembly-CSharp")
                .Replace("\\", string.Empty);

            return JsonConvert.DeserializeObject<T>(str, DefaultSetting);
        }

        public static int ToInt(this JsonData data) => ToInt(data.ToString());

        public static float ToFloat(this JsonData data) => ToFloat(data.ToString());
    }
}