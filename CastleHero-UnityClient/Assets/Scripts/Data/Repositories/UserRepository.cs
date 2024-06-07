using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.InGame;
using RGLabs.Lobby.Behaviours;
using RGLabs.Network.Model;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.Data.Repositories
{
    public class UserRepository
    {
        [Serializable]
        public class ArrayWrap<T>
        {
            public T[] array;
        }

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

            characters = new(LoadArray<UnitInfo>(CharactersKey, TestData()));
            characters
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x=> SaveArray(CharactersKey, x));
            
            items = new(LoadArray<IItem>(InventoryKey));
            items
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x => SaveArray(InventoryKey, x));
            
            fieldCharacters = new(LoadArray<FieldCharacter>(FieldCharactersKey));
            fieldCharacters
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x => SaveArray(FieldCharactersKey, x));
        }

        public void ApplyFieldCharacters(IEnumerable<UnitBehaviour> units)
        {
            fieldCharacters.Clear();
            foreach (var unit in units)
            {
                int index = characters.IndexOf(unit.Info);
                if (!index.IsValidIndex())
                    continue;
                
                fieldCharacters.Add(new FieldCharacter
                {
                    index = index,
                    position = unit.position
                });
            }
            
            SaveArray(FieldCharactersKey, fieldCharacters.ToArray());
        }

        private static int Load(string key, int defaultVal = -1) => PlayerPrefs.GetInt(key, defaultVal);

        private static void Save(string key, int value) => PlayerPrefs.SetInt(key, value);

        private static T[] LoadArray<T>(string key, string defaultVal = "")
        {
            var wrap = JsonUtility.FromJson<ArrayWrap<T>>(PlayerPrefs.GetString(key, defaultVal));
            return wrap == null ? Array.Empty<T>() : wrap.array;
        }

        private static void SaveArray<T>(string key, IEnumerable<T> value)
        {
            var wrap = new ArrayWrap<T> { array = value.ToArray() };
            var data = JsonUtility.ToJson(wrap);

            Debug.Log(data);
            PlayerPrefs.SetString(key, data);
        }

        private static string TestData()
        {
            var array = new[]
            {
                new UnitInfo
                {
                    lv = 1,
                    rate = 1,
                    id = 10000
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

            return JsonUtility.ToJson(new ArrayWrap<UnitInfo> { array = array });
        }
    }
}