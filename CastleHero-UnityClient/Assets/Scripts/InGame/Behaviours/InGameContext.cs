using System;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours.Player;
using RGLabs.InGame.Behaviours.Wave;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace RGLabs.InGame.Behaviours
{
    public class InGameContext : MonoBehaviour
    {
        public event Action OnEnd;

        public static PoolContainer pools;
        public static Streams streams;
        public static Camp camp;
        public static DBReference db;

        [SerializeField] private Camp _camp;
        [SerializeField] private WaveSystemBehaviour waveSystem;

        private UnitStreamHandler _unitStreamHandler;
        
        public async void Awake()
        {
            InitPool();
            InitStreams();

            _unitStreamHandler = new UnitStreamHandler(streams.atk, streams.heal);
            
            await InitResource();
            await InitSpawn();

            waveSystem.Init();
            RunGame();
        }

        private void Update()
        {
            streams.Update();
        }

        public void Retry()
        {
            pools.Dispose();
            streams.Dispose();
            SceneManager.LoadScene("SampleScene");
        }

        private void InitPool() => pools = new();
        
        private void InitStreams()
        {
            streams = new Streams()
            {
                spawnEvent = { processPerFrame = 1 },
                release = { processPerFrame = 10 },
                atk = { processPerFrame = 1},
                heal = { processPerFrame = 1 },
            };
        }

        private async UniTask InitSpawn()
        {
            await _camp.Init();

            _camp.OnDestroyed -= StopGame;
            _camp.OnDestroyed += StopGame;
            camp = _camp;
        }

        private async UniTask InitResource()
        {
            await Addressables.InitializeAsync();
            var catalogs = await Addressables.CheckForCatalogUpdates();
            foreach (var catalog in catalogs)
            {
                await Addressables.DownloadDependenciesAsync(catalog);
            }

            db = await Addressables.LoadAssetAsync<DBReference>("Common/DB.asset");
        }

        private void RunGame()
        {
            waveSystem.isRunning = true;
        }

        private void StopGame()
        {
            waveSystem.isRunning = false;
            OnEnd?.Invoke();
        }
    }
}