using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours.Wave;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.System;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.UI;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace RGLabs.InGame.Behaviours
{
    public class InGameBehaviour : MonoBehaviour
    {
        [field: SerializeField] public Formation Formation { get; private set; }

        [SerializeField] private UIStageSelect _stageSelect;
        [SerializeField] private UIConfigFormation _configFormation;

        [SerializeField] private WaveRunner _waveRunner;

        private InGameDB _db;

        private InGameRepository _inGameRepo;
        private UserRepository _userRepo;

        private IUnitFactory _unitFactory;
        private PoolContainer _pools;

        private void Awake()
        {
            _inGameRepo = new InGameRepository();
            _userRepo = new UserRepository();

            _pools = new PoolContainer();
            _unitFactory = new DefaultUnitFactory(_pools);

            InitAsync();
        }

        private async void InitAsync()
        {
            await InitResources();

            await Formation.Init(_db.characters, _inGameRepo, _unitFactory);

            MessageBroker.Default
                .Receive<UIStageSelect.Result>()
                .Subscribe(_ => OpenConfigFormation())
                .AddTo(this);

            MessageBroker.Default
                .Receive<UIConfigFormation.Result>()
                .Subscribe(x =>
                {
                    if (x.completed)
                        StartGame();
                    else
                        OpenStageSelect();
                })
                .AddTo(this);
            
            OpenStageSelect();
        }
        
        private async UniTask InitResources()
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

            _db = await InGameDB.Load();
        }

        private void OpenStageSelect() => _stageSelect.Open(_inGameRepo, _db);

        private void OpenConfigFormation() => _configFormation.Open(_userRepo, _db.characters, _unitFactory);

        private void StartGame()
        {
            _stageSelect.Dispose();

            _waveRunner.Init(_unitFactory, _db.monsters, _db.waves, _inGameRepo.castle.Value);
            _waveRunner.isRunning = true;

            foreach (var set in _inGameRepo.characterSet.Value)
            {
                set.unit.canMove = true;
                set.unit.canAttack = true;
            }
        }
    }
}