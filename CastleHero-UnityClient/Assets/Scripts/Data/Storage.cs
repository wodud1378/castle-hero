using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Flow;
using RGLabs.Common.Pattern;
using RGLabs.Data.Repositories;
using RGLabs.Network.DB;
using RGLabs.Unit.Factory;
using UnityEngine.AddressableAssets;

namespace RGLabs.Data
{
    public struct Entrance
    {
        public State state;
        public int stage;
    }
    
    public static class Storage
    {
        public static readonly UserRepository userRepository = new();
        public static readonly InGameRepository inGameRepository = new();
        public static DBCollections db;
        public static PoolContainer poolContainer;
        public static UnitFactory unitFactory;
        public static CastleFactory castleFactory;
        
        public static Entrance entranceData = new() { state = State.Lobby, };

        public static void Init(DBCollections database)
        {
            db = database;
            
            poolContainer = new();
            unitFactory = new UnitFactory();
            castleFactory = new CastleFactory();
        }
        
        public static async UniTask InitAsync()
        {
            await InitAddressable();

            poolContainer = new();
            unitFactory = new UnitFactory();
            castleFactory = new CastleFactory();
        }
        
        private static async UniTask InitAddressable()
        {
            await Addressables.InitializeAsync();
            var catalogs = await Addressables.CheckForCatalogUpdates();
            var tasks = new List<UniTask>();
            foreach (var catalog in catalogs)
            {
                var handle = Addressables.DownloadDependenciesAsync(catalog);
                tasks.Add(handle.ToUniTask());
            }

            await UniTask.WhenAll(tasks);
            
            tasks.Clear();
        }
    }
}