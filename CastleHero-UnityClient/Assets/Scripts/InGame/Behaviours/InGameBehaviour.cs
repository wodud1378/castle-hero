using System;
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

namespace RGLabs.InGame.Behaviours
{
    public class InGameBehaviour : MonoBehaviour
    {
        [SerializeField] private UIControl _uiControl;
        
        [SerializeField] private UIStageSelect _stageSelect;
        [SerializeField] private UIConfigFormation _configFormation;

        [SerializeField] private Formation _formation;
        [SerializeField] private SpriteRenderer _map;
        [SerializeField] private WaveRunner _waveRunner;

        private DBCollections _dbCollections;

        private InGameRepository _inGameRepo;
        private UserRepository _userRepo;

        private IUnitFactory _unitFactory;
        private PoolContainer _pools;

        private async void Awake()
        {
            _inGameRepo = new InGameRepository();
            _userRepo = new UserRepository();

            _pools = new PoolContainer();
            _unitFactory = new DefaultUnitFactory(_pools);
            
            await InitAsync();

            _uiControl.step.Subscribe(OnNextStep);
            
            _stageSelect.submit
                .OnClickAsObservable()
                .Subscribe(OnStageSelectSubmit);

            _configFormation.back
                .OnClickAsObservable()
                .Subscribe(OnCancelStart);
        }

        private void OnCancelStart(UniRx.Unit _)
        {
            var step = _uiControl.step.Value;
            if (step != UIControl.Step.Stage) return;

            _uiControl.step.Value = UIControl.Step.Lobby;
        }

        private void OnStageSelectSubmit(UniRx.Unit _)
        {
            var step = _uiControl.step.Value;
            if (step == UIControl.Step.InGame) return;

            _uiControl.step.Value = step + 1;
        }

        private void OnNextStep(UIControl.Step step)
        {
            switch (step)
            {
                case UIControl.Step.Lobby:
                    _stageSelect.Set(_userRepo, _dbCollections);
                    break;
                case UIControl.Step.Stage:
                    _configFormation.Set(_userRepo, _dbCollections.characters, _unitFactory);
                    break;
                case UIControl.Step.InGame:
                    _stageSelect.Dispose();
                    _configFormation.Dispose();
                    StartGame();
                    break;
            }

            _stageSelect.enabled = step == UIControl.Step.Lobby;
            _configFormation.enabled = step == UIControl.Step.Stage;
        }

        private async UniTask InitAsync()
        {
            await InitResources();
            await _formation.Init(_dbCollections.characters, _userRepo, _inGameRepo, _unitFactory);
            
            _stageSelect.Init();
            _stageSelect.Set(_userRepo, _dbCollections);
            
            _configFormation.Init();
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

            _dbCollections = await DBCollections.Load();
        }
        
        private void StartGame()
        {
            _waveRunner.Init(_unitFactory, _dbCollections.monsters, _dbCollections.waves, _inGameRepo.castle.Value);
            _waveRunner.isRunning = true;

            foreach (var unit in _inGameRepo.characters.Value)
            {
                unit.canMove = true;
                unit.canAttack = true;
            }
        }
    }
}