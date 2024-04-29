using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Data.Repositories;
using UniRx;
using UnityEngine.AddressableAssets;

namespace RGLabs.InGame.Data
{
    public struct StorageInitDone
    {
    }
    
    public static class Storage
    {
        public static readonly UserRepository userRepository = new();
        public static readonly InGameRepository inGameRepository = new();
        
        public static DBCollections DB { get; private set; }
        
        public static async UniTask InitAsync()
        {
            await InitAddressable();
            
            DB = await DBCollections.Load();
            
            MessageBroker.Default.Publish(new StorageInitDone());
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