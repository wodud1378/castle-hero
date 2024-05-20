using System;
using Cysharp.Threading.Tasks;
using RGLabs.Data.DB;
using RGLabs.Data.Load;
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
        public UnitDB units;
        public UnitLevelDB levels;
        public SkillDB skills;

        private readonly LocalDataLoader _localLoader;
        
        private DBCollections()
        {
            _localLoader = new();
        }

        private async UniTask Init()
        {
            stages = _localLoader.Load<StageDB>();
            waves = _localLoader.Load<WaveDB>();
            rewards = _localLoader.Load<RewardDB>();
            items = _localLoader.Load<ItemDB>();
            units = _localLoader.Load<UnitDB>();
            levels = _localLoader.Load<UnitLevelDB>();
            skills = _localLoader.Load<SkillDB>();
            
            units.CacheUnitSizes();
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