using System;
using BackEnd;
using LitJson;
using Newtonsoft.Json;

namespace BackendFunction
{
    public partial class BFunc
    {
        private void ConsumeDia(ref int freeDia, ref int paidDia, int amount)
        {
            if (freeDia > amount)
            {
                freeDia -= amount;
            }
            else
            {
                int remain = amount - freeDia;
                freeDia = 0;
                paidDia -= remain;
            }
        }

        static ItemType ToItemType(int id)
        {
            int val = id / 10000;
            if (val < (int)ItemType.Consumable)
                return ItemType.Equipment;

            return (ItemType)val;
        }

        static JsonData LoadChart(int id) => LoadChart(id.ToString());

        static JsonData LoadChart(string id) => Backend.Chart.GetChartContents(id).GetFlattenJSON()["rows"];

        static bool TryLoad(JsonData data, string key, out int value)
        {
            if (!data.ContainsKey(key))
            {
                value = 0;
                return false;
            }

            value = data[key].ToInt();
            return true;
        }

        static bool TryLoad(JsonData data, string key, out float value)
        {
            if (!data.ContainsKey(key))
            {
                value = 0f;
                return false;
            }

            value = data[key].ToFloat();
            return true;
        }

        static bool TryLoad(JsonData data, string key, out string value)
        {
            if (!data.ContainsKey(key))
            {
                value = string.Empty;
                return false;
            }

            value = data[key].ToString();
            return true;
        }
    }

    public static class BackendFunctionExtensions
    {
        private static JsonSerializerSettings _defaultSetting = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj, _defaultSetting);

        public static T Cast<T>(this JsonData data) => JsonConvert.DeserializeObject<T>(data.ToString(), _defaultSetting);

        public static int ToInt(this JsonData data) => ToInt(data.ToString());

        public static float ToFloat(this JsonData data) => ToFloat(data.ToString());

        public static int ToInt(this string value)
        {
            if (!int.TryParse(value, out var result))
                result = 0;

            return result;
        }

        public static float ToFloat(this string value)
        {
            if (!float.TryParse(value, out var result))
                result = 0f;

            return result;
        }

        public static void BinarySearch(this JsonData jsonData, Action<JsonData> onFound)
        {
            int count = jsonData.Count;
            BinarySearch(jsonData, onFound, 0, count);
        }

        private static void BinarySearch(
            JsonData jsonData,
            Action<JsonData> action,
            int startIndex,
            int count
        )
        {
            if (count == 0)
                return;

            int midIndex = startIndex + count / 2;
            JsonData midData = jsonData[midIndex];

            // 중간 데이터에 대해 Action 실행
            action(midData);

            // 왼쪽 절반 탐색
            BinarySearch(jsonData, action, startIndex, count / 2);

            // 오른쪽 절반 탐색
            BinarySearch(jsonData, action, midIndex + 1, count - (count / 2) - 1);
        }

        public static JsonData BinarySerach(this JsonData jsonData, Predicate<JsonData> predicate)
        {
            int count = jsonData.Count;
            return BinarySearch(jsonData, predicate, 0, count);
        }

        private static JsonData BinarySearch(
            JsonData jsonData,
            Predicate<JsonData> predicate,
            int startIndex,
            int count
        )
        {
            if (count == 0)
                return null;

            int midIndex = startIndex + count / 2;
            JsonData midData = jsonData[midIndex];

            if (predicate(midData))
            {
                return midData;
            }

            JsonData leftResult = BinarySearch(jsonData, predicate, startIndex, count / 2);
            if (leftResult != null)
            {
                return leftResult;
            }

            return BinarySearch(jsonData, predicate, midIndex + 1, (count - 1) / 2);
        }
    }
}
