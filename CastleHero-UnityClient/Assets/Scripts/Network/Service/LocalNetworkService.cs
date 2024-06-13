using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Model;
using UnityEngine;

namespace RGLabs.Network.Service
{
    public class LocalNetworkService : INetworkService
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

        public UniTask<UserInfo> Login()
        {
            var info = new UserInfo
            {
                stage = Load(SavedStageKey, 1),
                castleLv = Load(CastleKey, 1),
                characters = LoadArray<UnitInfo>(CharactersKey, TemplateCharacters()),
                fieldCharacters = LoadArray<FieldCharacter>(FieldCharactersKey),
                items = LoadArray<IItem>(InventoryKey)
            };

            return UniTask.FromResult(info);
        }

        public UniTask LoadTables()
        {
            throw new NotImplementedException();

        }

        public UniTask<CharacterLevelUp> LevelUp(int id, IEnumerable<Consume> items)
        {
            var characters = LoadArray<UnitInfo>(CharactersKey);
            int index = Array.FindIndex(characters, x => x.id == id);
            
            throw new NotImplementedException();

        }
        
        public UniTask<CharacterUpgrade> Upgrade(int id, IEnumerable<Consume> items)
        {
            throw new NotImplementedException();

        }

        public UniTask Consume(IEnumerable<Consume> items)
        {
            throw new System.NotImplementedException();
        }

        public UniTask Consume(Consume items)
        {
            throw new System.NotImplementedException();
        }

        public UniTask<StageClear> StageClear(int stage)
        {
            throw new System.NotImplementedException();
        }

        public UniTask SetStage(int stage)
        {
            throw new NotImplementedException();
        }

        private static int Load(string key, int defaultVal = -1) => PlayerPrefs.GetInt(key, defaultVal);

        private static void Save(string key, int value) => PlayerPrefs.SetInt(key, value);

        private static T[] LoadArray<T>(string key, string defaultVal = "")
        {
            var wrap = JsonUtility.FromJson<ArrayWrap<T>>(PlayerPrefs.GetString(key, defaultVal));
            if (wrap == null)
                return null;

            return wrap.array;
        }

        private static void SaveArray<T>(string key, IEnumerable<T> value)
        {
            var wrap = new ArrayWrap<T> { array = value.ToArray() };
            var data = JsonUtility.ToJson(wrap);

            Debug.Log(data);
            PlayerPrefs.SetString(key, data);
        }

        private static string TemplateCharacters()
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