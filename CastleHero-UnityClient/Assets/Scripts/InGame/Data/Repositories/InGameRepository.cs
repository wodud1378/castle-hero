using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.DB;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.InGame.Data.Repositories
{
    public class InGameDB
    {
        public static async UniTask<InGameDB> Load()
        {
            if (_loaded == null)
            {
                _loaded = new InGameDB();

                await _loaded.Init();
            }

            return _loaded;
        }

        private static InGameDB _loaded = null;

        private const string DBRoot = "Common/DB/";

        public StageDB stages;
        public WaveDB waves;
        public RewardDB rewards;
        public ItemDB items;
        public UnitDB characters;
        public UnitDB monsters;

        private InGameDB()
        {
        }

        private async UniTask Init()
        {
            stages = await LoadDB<StageDB>("Stages");
            characters = await LoadDB<UnitDB>("Characters");
            monsters = await LoadDB<UnitDB>("Monsters");
            waves = await LoadDB<WaveDB>("Waves");
            rewards = await LoadDB<RewardDB>("Rewards");
            items = await LoadDB<ItemDB>("Items");
        }

        private async UniTask<T> LoadDB<T>(string name) =>
            await Addressables.LoadAssetAsync<T>($"{DBRoot}{name}.asset");
    }

    public class InGameRepository : IDisposable
    {
        private const string SavedStageKey = "saved-stage";
        private const string SavedCastleKey = "saved-castle";
        private const string SavedCharacterKey = "saved-characters";
        
        public readonly ReactiveProperty<int> savedCastle = new(Load(SavedCastleKey));
        public readonly ReactiveProperty<SavedUnit[]> savedCharacters = new(Load<SavedUnit[]>(SavedCharacterKey));
     
        public readonly ReactiveProperty<int> stage = new(Load(SavedStageKey));

        public readonly ReactiveProperty<UnitBehaviour> castle = new(null);
        public readonly ReactiveProperty<UnitSet[]> characterSet = new(null);

        public void SaveCharacters()
        {
            var list = new List<SavedUnit>();
            foreach (var character in characterSet.Value)
            {
                list.Add(new SavedUnit(character));
            }
            
            Save(SavedCharacterKey, list);
        }
        
        public void Dispose()
        {
        }

        private static int Load(string key) => PlayerPrefs.GetInt(key, -1);

        private static void Save(string key, int value) => PlayerPrefs.SetInt(key, value);
        
        private static T Load<T>(string key) => JsonUtility.FromJson<T>(PlayerPrefs.GetString(key));

        private static void Save<T>(string key, T value) => PlayerPrefs.SetString(key, JsonUtility.ToJson(value));
    }
}