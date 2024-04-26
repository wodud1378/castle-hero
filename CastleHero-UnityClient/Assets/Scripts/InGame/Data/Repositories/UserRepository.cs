using System;
using System.Collections.Generic;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.User;
using RGLabs.InGame.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.Data.Repositories
{
    public class UserRepository
    {
        private const string SavedStageKey = "saved-stage";
        private const string CharactersKey = "characters";
        private const string FieldCharactersKey = "characters-field";
        private const string CastleKey = "saved-castle";

        public readonly ReactiveProperty<int> stage;
        public readonly ReactiveProperty<int> castle;
        public readonly ReactiveProperty<Character[]> characters;
        public readonly ReactiveProperty<FieldCharacter[]> fieldCharacters;
        
        public UserRepository()
        {
            stage = new(Load(SavedStageKey, 1));
            stage.Subscribe(x => Save(SavedStageKey, x));

            castle = new(Load(CastleKey));
            castle.Subscribe(x => Save(CastleKey, x));

            characters = new(Load<Character[]>(CharactersKey, TestData()));
            characters.Subscribe(x => Save(CharactersKey, x));
            
            fieldCharacters = new(Load<FieldCharacter[]>(FieldCharactersKey));
            fieldCharacters.Subscribe(x => Save(FieldCharactersKey, x));
        }
        
        public void SaveFieldCharacters(UnitBehaviour[] units)
        {
            int length = units.Length;
            var array = new FieldCharacter[length];
            for (int i = 0; i < length; ++i)
            {
                var unit = units[i];
                int index = Array.FindIndex(characters.Value, (x) => x.id == unit.Id);
                if(!index.IsValidIndex(characters.Value))
                    continue;

                array[i] = new FieldCharacter
                {
                    index = index,
                    position = unit.transform.position
                };
            }

            fieldCharacters.Value = array;
        }
        
        private static int Load(string key, int defaultVal = -1) => PlayerPrefs.GetInt(key, defaultVal);

        private static void Save(string key, int value) => PlayerPrefs.SetInt(key, value);
        
        private static T Load<T>(string key, string defaultVal = "") => JsonUtility.FromJson<T>(PlayerPrefs.GetString(key, defaultVal));

        private static void Save<T>(string key, T value) => PlayerPrefs.SetString(key, JsonUtility.ToJson(value));

        private static string TestData()
        {
            var array = new[]
            {
                new Character
                {
                    lv = 1,
                    grade = 1,
                    id = 10021
                }
            };

            return JsonUtility.ToJson(array);
        }
    }
}