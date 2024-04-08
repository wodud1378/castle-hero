using Cysharp.Threading.Tasks;
using RGLabs.Common.ResourceManagement;
using RGLabs.InGame.Behaviours.Player;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Behaviours.Wave;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.InGame
{
    public class InGameContext : MonoBehaviour
    {
        public static readonly PoolContainer Pools = new();
        public static readonly AssetBundleResource Resource = new();
        
        [SerializeField] private DBReference _dbReference;
        [SerializeField] private Camp _camp;
        [SerializeField] private WaveController _wave;
        
        public async void Awake()
        {
            await InitAsync();
            
            RunGame();
        }

        private async UniTask InitAsync()
        {
            await InitResource();
            await _camp.Init(_dbReference.characters);
        }
        
        private async UniTask InitResource()
        {
            await Addressables.InitializeAsync();
            var catalogs = await Addressables.CheckForCatalogUpdates();
            foreach (var catalog in catalogs)
            {
                Debug.Log(catalog);
                await Addressables.DownloadDependenciesAsync(catalog);
            }
        }

        private void RunGame()
        {
            _wave.IsRunning = true;
        }
    }
}