using System;
using Cysharp.Threading.Tasks;
using RGLabs.Data.DB;
using RGLabs.InGame;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using UniRx;
using UnityEngine.AddressableAssets;

namespace RGLabs.Data.Repositories
{
    public class DBCollections
    {
        public static async UniTask<DBCollections> Load()
        {
            if (_loaded == null)
            {
                _loaded = new DBCollections();

                await _loaded.Init();
            }

            return _loaded;
        }

        private static DBCollections _loaded = null;

        private const string DBRoot = "DB/";

        public StageDB stages;
        public WaveDB waves;
        public RewardDB rewards;
        public ItemDB items;
        public UnitDB characters;
        public UnitDB monsters;
        public UnitLevelDB characterLevels;
        public UnitLevelDB monsterLevels;

        private DBCollections()
        {
        }

        private async UniTask Init()
        {
            stages = await LoadDB<StageDB>("Stages");
            waves = await LoadDB<WaveDB>("Waves");
            rewards = await LoadDB<RewardDB>("Rewards");
            items = await LoadDB<ItemDB>("Items");
            characters = await LoadDB<UnitDB>("Characters");
            monsters = await LoadDB<UnitDB>("Monsters");
            characterLevels = await LoadDB<UnitLevelDB>("CharacterLevels");
            monsterLevels = await LoadDB<UnitLevelDB>("MonsterLevels");
            
            monsters.CacheUnitSizes();
        }

        private async UniTask<T> LoadDB<T>(string name) =>
            await Addressables.LoadAssetAsync<T>($"{DBRoot}{name}.asset");
    }

    public class InGameRepository : IDisposable
    {
        public readonly ReactiveProperty<UnitBehaviour> castle = new(null);
        public readonly ReactiveProperty<UnitBehaviour[]> characters = new(null);

        public readonly ReactiveCollection<WaitRecover> recovers = new();

        public void Dispose()
        {
            castle.Value = null;
            characters.Value = null;
            recovers.Clear();
            
            castle.Dispose();
            characters.Dispose();
        }
    }
}