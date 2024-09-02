using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Common;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Unit;

namespace RGLabs.Utility
{
    public static class ItemHelper
    {
        private static readonly Dictionary<int, List<int>> RandomIdCache = new();

        public static List<int> GetRelatedItemIds(this int id)
        {
            if (!id.IsRandomItem())
                return new() { id };

            if (!RandomIdCache.TryGetValue(id, out var cache))
            {
                cache = new List<int>();
                FilterItemIds(id, cache);

                RandomIdCache[id] = cache;
            }

            return cache;
        }

        public static void GetRelatedItemIds(this int id, List<int> list)
        {
            list ??= new();

            if (!id.IsRandomItem())
            {
                list.Add(id);
                return;
            }

            if (!RandomIdCache.TryGetValue(id, out var cache))
            {
                cache = new List<int>();
                FilterItemIds(id, cache);

                RandomIdCache[id] = cache;
            }

            list.AddRange(cache);
        }

        public static bool HasEnoughItems(this List<IItem> items, int id, int quantity)
        {
            var exist = items.FirstOrDefault(x => x.ItemId == id);
            if (exist == null)
                return false;

            return exist.Quantity >= quantity;
        }

        public static bool TryConsumeItem(this List<IItem> items, int id, int quantity)
        {
            var item = items.FirstOrDefault(x => x.ItemId == id);
            if (item == null || item.Quantity < quantity)
                return false;

            if (item.Quantity == quantity)
                items.Remove(item);
            else
                item.Quantity -= quantity;
            
            return true;
        }
        
        public static bool TryConsumeItem(this List<IItem> items, IItem item) => items.TryConsumeItem(item, item.Quantity);

        public static bool TryConsumeItem(this List<IItem> items, IItem item, int quantity)
        {
            var exist = items.Contains(item)
                ? item
                : items.FirstOrDefault(x => x.ItemId == item.ItemId);
            
            if (exist == null || exist.Quantity < quantity)
                return false;

            if (exist.Quantity == quantity)
                items.Remove(exist);
            else
                exist.Quantity -= quantity;
            
            return true;
        }

        private static void FilterItemIds(int id, List<int> result)
        {
            // a의 각 자릿수
            int length = (int)Math.Log10(id) + 1;
            Storage.db.items.BinarySearch(x =>
            {
                if (MatchesCriteria(id, x.Id, length))
                    return;

                result.Add(x.Id);
            });
        }

        private static bool MatchesCriteria(int a, int b, int length)
        {
            for (int i = 0; i < length; i++)
            {
                int aDigit = a / (int)Math.Pow(10, length - i - 1) % 10;
                int idDigit = b / (int)Math.Pow(10, length - i - 1) % 10;

                if (aDigit != 0 && aDigit != idDigit)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsCurrency(this int id)
        {
            return id is Constants.GoldId or Constants.FreeDiaId or Constants.PaidDiaId;
        }

        public static void AddOrNew(this List<IItem> exists, List<IItem> targets)
        {
            foreach (var item in targets)
            {
                exists.AddOrNew(item);
            }
        }

        public static void AddOrNew(this List<IItem> items, IItem item)
        {
            var exist = item is EquipItem ? null : items.Find(x => x.ItemId == item.ItemId);

            if (exist == null)
                items.Add(item);
            else
                exist.Quantity += item.Quantity;
        }

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

        public static bool IsRandomItem(this int id) => id % 10 == 0;

        public static bool IsEquipItem(this int id)
        {
            int val = id / 10000;
            return val is >= 1 and <= 4;
        }

        public static bool IsChestItem(this int id) => id / 10000 == 7;
    }
}