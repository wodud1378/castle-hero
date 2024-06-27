using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RGLabs.Network.Model;
using UnityEngine;

namespace RGLabs.Network.Utility
{
    public static class LocalUtility
    {
        public static int Load(this string key, int defaultVal = -1) => PlayerPrefs.GetInt(key, defaultVal);

        public static void Save(this string key, int value) => PlayerPrefs.SetInt(key, value);

        public static T[] LoadAsArray<T>(this string key, string defaultVal = "")
        {
            var value = PlayerPrefs.GetString(key, defaultVal);
            var array = typeof(T).IsInterface
                ? JsonConvert.DeserializeObject<T[]>(value,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
                : JsonConvert.DeserializeObject<T[]>(value);

            return array ?? Array.Empty<T>();
        }

        public static void SaveAsArray<T>(this string key, IEnumerable<T> value)
        {
            string json = typeof(T).IsInterface
                ? JsonConvert.SerializeObject(value, Formatting.None,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
                : JsonConvert.SerializeObject(value);

            Debug.Log(json);
            PlayerPrefs.SetString(key, json);
        }
        
        public static string TestItem()
        {
            var array = new IItem[]
            {
                new EquipItem
                {
                    Id = 1,
                    ItemId = 30001,
                    Quantity = 1,
                    character = 0,
                    slot = 0,
                    stats = new[] { 1, 2, 3 },
                    values = new[] { 150, 0.3f, 0.3f }
                },
                new EquipItem
                {
                    Id = 2,
                    ItemId = 30002,
                    Quantity = 1,
                    character = 0,
                    slot = 1,
                    stats = new[] { 0 },
                    values = new[] { 100f }
                },
                new EquipItem
                {
                    Id = 3,
                    ItemId = 30002,
                    Quantity = 1,
                    character = 0,
                    slot = 2,
                    stats = new[] { 4 },
                    values = new[] { 0.1f }
                },
                new EquipItem
                {
                    Id = 4,
                    ItemId = 30002,
                    Quantity = 1,
                    character = 0,
                    slot = 3,
                    stats = new[] { 5 },
                    values = new[] { 0.1f }
                },
                new ConsumableItem
                {
                    Id = 5,
                    ItemId = 51001,
                    Quantity = 3,
                    consumeOption = 1
                },
                new ConsumableItem
                {
                    Id = 6,
                    ItemId = 52001,
                    Quantity = 5,
                    consumeOption = 2
                },
                new ConsumableItem
                {
                    Id = 7,
                    ItemId = 53001,
                    Quantity = 10,
                    consumeOption = 3
                },
                new ConsumableItem
                {
                    Id = 8,
                    ItemId = 54001,
                    Quantity = 5,
                    consumeOption = 4
                },
                new Item
                {
                    Id = 9,
                    ItemId = 60001,
                    Quantity = 150,
                }
            };

            return JsonConvert.SerializeObject(array, Formatting.None,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }

        public static string TestCharacter()
        {
            var array = new[]
            {
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10000,
                    equipments = new EquipItem[]
                    {
                        new()
                        {
                            ItemId = 30001,
                            Quantity = 1,
                            character = 10000,
                            slot = 0,
                            stats = new[]
                            {
                                1, 2, 3
                            },
                            values = new[]
                            {
                                150, 0.3f, 0.3f
                            }
                        }
                    },
                },
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10001
                },
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10021
                },
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10023
                },
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10034
                },
            };

            return JsonConvert.SerializeObject(array);
        }
    }
}