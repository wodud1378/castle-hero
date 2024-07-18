using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RGLabs.Network.Shared;
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

        public readonly ReactiveCollection<FieldUnit> fieldCharacters;
        public readonly ReactiveCollection<UnitInfo> characters;
        
        public readonly ReactiveProperty<int> paidDia;
        public readonly ReactiveProperty<int> freeDia;
        public readonly ReactiveProperty<int> gold;
        public readonly ReactiveCollection<IItem> items;

        public UserRepository(UserData userData)
        {
            var info = userData.info;
            stage = new(info.stage);
            focusedStage = new(info.focusedStage);
            castleLv = new(info.castleLv);

            fieldCharacters = new(userData.formation.fieldUnits);
            characters = new(userData.characters.units);
            items = new(userData.inventory.items);

            var currency = userData.currency;
            paidDia = new(currency.paidDia);
            freeDia = new(currency.freeDia);
            gold = new(currency.gold);
        }


        public void UpdateCurrency(Currency currency)
        {
            paidDia.Value = currency.paidDia;
            freeDia.Value = currency.freeDia;
            gold.Value = currency.gold;
        }

        public void UpdateCharacter(UnitInfo unit) => UpdateElement(unit, x => x.id == unit.id, characters);
        public void UpdateItem(IItem item) => UpdateElement(item, x => x.ItemId == item.ItemId, items);

        private void UpdateElement<T>(T value, Predicate<T> predicate, ReactiveCollection<T> collection)
        {
            var exist = collection.FirstOrDefault(predicate.Invoke);
            if (exist == null)
                return;

            int index = collection.IndexOf(exist);
            collection.Remove(exist);
            collection.Insert(index, value);
        }
        
        public void ApplyFieldCharacters(IEnumerable<UnitBehaviour> units)
        {
            fieldCharacters.Clear();
            foreach (var unit in units)
            {
                var position = unit.position;
                fieldCharacters.Add(new FieldUnit
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