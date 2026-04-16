using System.Collections.Generic;
using System.Linq;
using CastleHero.Common;
using CastleHero.Data;
using CastleHero.Network.Shared;
using UnityEngine;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.Utility
{
    /// <summary>
    /// Item helper methods that depend only on Data types.
    /// Equipment-specific helpers that depend on Unit component types are in GamePlay/Unit/EquipmentHelper.cs.
    /// </summary>
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

        public static bool TryConsumeItem(this List<IItem> items, IItem item) =>
            items.TryConsumeItem(item, item.Quantity);

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

        public static void Join(this List<IItem> exists, List<IItem> targets)
        {
            foreach (var item in targets)
                exists.Join(item);
        }

        public static void Join(this List<IItem> items, IItem item)
        {
            var exist = item is EquipItem ? null : items.Find(x => x.ItemId == item.ItemId);

            if (exist == null)
                items.Add(item);
            else
                exist.Quantity += item.Quantity;
        }

        public static bool IsRandomItem(this int id) => id / 1000 != 61 && id % 10 == 0;

        public static bool IsEquipItem(this int id)
        {
            int val = id / 10000;
            return val is >= 1 and <= 4;
        }

        public static bool IsCurrency(this int id)
        {
            return id is Constants.GoldId or Constants.FreeDiaId or Constants.PaidDiaId;
        }

        private static void FilterItemIds(int id, List<int> result)
        {
            ServiceLocator.Get<IDBProvider>().Items.ForEach(x =>
            {
                if (!IsRelated(id, x.Id))
                    return;

                result.Add(x.Id);
            });
        }

        private static bool IsRelated(int origin, int compare)
        {
            if (origin == compare)
                return false;

            var splitA = Split(origin);
            var splitB = Split(compare);

            int length = splitA.Length;
            if (length != splitB.Length)
                return false;

            bool isMatch = true;
            for (int i = 0; i < length && isMatch; ++i)
            {
                if (splitA[i] != 0)
                {
                    isMatch = splitA[i] == splitB[i];
#if UNITY_EDITOR
                    if (!isMatch)
                        Debug.Log($"{origin}/{compare} not matches in index{i}, values are {splitA[i]}, {splitB[i]}");
#endif
                }
            }

#if UNITY_EDITOR
            if (isMatch)
                Debug.Log($"{origin}/{compare} matches");
#endif

            return isMatch;
        }

        private static int[] Split(int value)
        {
            int digitCount = Mathf.FloorToInt(Mathf.Log10(value)) + 1;
            var array = new int[digitCount];
            for (int i = digitCount - 1; i >= 0; --i)
            {
                array[i] = value % 10;
                value /= 10;
            }

            return array;
        }
    }
}
