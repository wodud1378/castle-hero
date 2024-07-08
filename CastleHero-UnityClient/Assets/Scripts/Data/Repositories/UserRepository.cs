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
        public readonly ReactiveProperty<int> stage;
        public readonly ReactiveProperty<int> focusedStage;
        public readonly ReactiveProperty<int> castleLv;

        public readonly ReactiveCollection<FieldCharacter> fieldCharacters;
        public readonly ReactiveCollection<UnitInfo> characters;
        
        public readonly ReactiveProperty<int> gold;
        public readonly ReactiveProperty<int> freeDia;
        public readonly ReactiveProperty<int> paidDia;
        public readonly ReactiveCollection<IItem> items;

        public UserRepository(UserInfo userInfo)
        {
            stage = new(userInfo.stage);
            focusedStage = new(userInfo.focusedStage);
            castleLv = new(userInfo.castleLv);

            fieldCharacters = new(userInfo.fieldCharacters);
            characters = new(userInfo.characters);
            items = new(userInfo.items);

            gold = new(userInfo.gold);
            freeDia = new(userInfo.freeDia);
            paidDia = new(userInfo.paidDia);
        }

        // public UserRepository()
        // {
        //     stage = new(Load(SavedStageKey, 1));
        //     stage
        //         .ThrottleFrame(1)
        //         .Subscribe(x => Save(SavedStageKey, x));
        //
        //     castleLv = new(Load(CastleKey, 1));
        //     castleLv
        //         .ThrottleFrame(1)
        //         .Subscribe(x => Save(CastleKey, x));
        //
        //     characters = new(LoadAsArray<UnitInfo>(CharactersKey, TestCharacter()));
        //     characters
        //         .ChangeAsObservable()
        //         .ThrottleFrame(1)
        //         .Subscribe(x => SaveAsArray(CharactersKey, x));
        //
        //     fieldCharacters = new(LoadAsArray<FieldCharacter>(FieldCharactersKey));
        //     fieldCharacters
        //         .ChangeAsObservable()
        //         .ThrottleFrame(1)
        //         .Subscribe(x => SaveAsArray(FieldCharactersKey, x));
        //     
        //     items = new(LoadAsArray<IItem>(InventoryKey, TestItem()));
        //     items
        //         .ChangeAsObservable()
        //         .ThrottleFrame(1)
        //         .Subscribe(x => SaveAsArray(InventoryKey, x));
        //
        //     gold = new(Load(GoldKey, 0));
        //     gold
        //         .ThrottleFrame(1)
        //         .Subscribe(x => Save(GoldKey, x));
        //     
        //     freeDia = new(Load(FreeDiaKey, 0));
        //     freeDia
        //         .ThrottleFrame(1)
        //         .Subscribe(x => Save(FreeDiaKey, x));
        //     
        //     paidDia = new(Load(PaidDiaKey, 0));
        //     paidDia
        //         .ThrottleFrame(1)
        //         .Subscribe(x => Save(PaidDiaKey, x));
        // }

        public void ApplyFieldCharacters(IEnumerable<UnitBehaviour> units)
        {
            fieldCharacters.Clear();
            foreach (var unit in units)
            {
                var position = unit.position;
                fieldCharacters.Add(new FieldCharacter
                {
                    id = unit.Id,
                    x = position.x,
                    y = position.y,
                });
            }
        }

        public IEnumerable<EquipItem> EquipItems(IList<string> guids)
        {
            return items
                .OfType<EquipItem>()
                .Where(x => guids.Contains(x.Guid));
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

        private static string TestCharacter()
        {
            var array = new[]
            {
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10000,
                },
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10001,
                },
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10006
                },
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10010
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
                    id = 10033
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