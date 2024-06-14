using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RGLabs.Network.Model;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.Data.Repositories
{
    public class UserRepository
    {
        private const string SavedStageKey = "saved-stage";
        private const string CharactersKey = "characters";
        private const string FieldCharactersKey = "characters-field";
        private const string CastleKey = "saved-castle";
        private const string InventoryKey = "inventory";

        public readonly ReactiveProperty<int> stage;
        public readonly ReactiveProperty<int> castleLv;

        public readonly ReactiveCollection<FieldCharacter> fieldCharacters;
        public readonly ReactiveCollection<UnitInfo> characters;
        public readonly ReactiveCollection<IItem> items;

        public UserRepository(UserInfo userInfo)
        {
            stage = new(userInfo.stage);
            castleLv = new(userInfo.castleLv);

            fieldCharacters = new(userInfo.fieldCharacters);
            characters = new(userInfo.characters);
            items = new(userInfo.items);
        }

        public UserRepository()
        {
            stage = new(Load(SavedStageKey, 1));
            stage
                .ThrottleFrame(1)
                .Subscribe(x => Save(SavedStageKey, x));

            castleLv = new(Load(CastleKey, 1));
            castleLv
                .ThrottleFrame(1)
                .Subscribe(x => Save(CastleKey, x));

            characters = new(LoadAsArray<UnitInfo>(CharactersKey, TestCharacter()));
            characters
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x => SaveAsArray(CharactersKey, x));

            items = new(LoadAsArray<IItem>(InventoryKey, TestItem()));
            items
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x => SaveAsArray(InventoryKey, x));

            fieldCharacters = new(LoadAsArray<FieldCharacter>(FieldCharactersKey));
            fieldCharacters
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x => SaveAsArray(FieldCharactersKey, x));
        }

        public void ApplyFieldCharacters(IEnumerable<UnitBehaviour> units)
        {
            fieldCharacters.Clear();
            foreach (var unit in units)
            {
                int index = characters.IndexOf(unit.Info);
                if (!index.IsValidIndex())
                    continue;

                var position = unit.position;
                fieldCharacters.Add(new FieldCharacter
                {
                    index = index,
                    x = position.x,
                    y = position.y,
                });
            }
        }

        private static int Load(string key, int defaultVal = -1) => PlayerPrefs.GetInt(key, defaultVal);

        private static void Save(string key, int value) => PlayerPrefs.SetInt(key, value);

        private static T[] LoadAsArray<T>(string key, string defaultVal = "")
        {
            var value = PlayerPrefs.GetString(key, defaultVal);
            var array = typeof(T).IsInterface
                ? JsonConvert.DeserializeObject<T[]>(value,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
                : JsonConvert.DeserializeObject<T[]>(value);

            return array ?? Array.Empty<T>();
        }

        private static void SaveAsArray<T>(string key, IEnumerable<T> value)
        {
            string json = typeof(T).IsInterface
                ? JsonConvert.SerializeObject(value, Formatting.None,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All })
                : JsonConvert.SerializeObject(value);

            Debug.Log(json);
            PlayerPrefs.SetString(key, json);
        }

        private static string TestItem()
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

        private static string TestCharacter()
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