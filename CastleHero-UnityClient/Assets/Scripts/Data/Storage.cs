using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Data.Repositories;
using RGLabs.Lobby.UI;
using UnityEngine.AddressableAssets;

namespace RGLabs.Data
{
    public struct LobbyStartUp
    {
        public UILobby.Step step;
        public int stage;
    }
    
    public static class Storage
    {
        public static readonly UserRepository userRepository = new();
        public static readonly InGameRepository inGameRepository = new();

        public static LobbyStartUp StartUpData = new();
        
        public static DBCollections DB { get; private set; }
        
        public static async UniTask InitAsync()
        {
            await InitAddressable();
            
            DB = await DBCollections.Load();
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