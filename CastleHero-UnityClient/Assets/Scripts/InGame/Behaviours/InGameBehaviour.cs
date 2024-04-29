using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Pattern;
using RGLabs.InGame.Behaviours.Wave;
using RGLabs.InGame.Data;
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
            await Storage.InitAsync();
            
            _inGameRepo = Storage.inGameRepository;
            _userRepo = Storage.userRepository;
            _dbCollections = Storage.DB;
            _pools = new PoolContainer();
            _unitFactory = new DefaultUnitFactory(_pools);

            await _formation.Init(_dbCollections.characters, _userRepo, _inGameRepo, _unitFactory);
            
            _uiControl.Init();
            _uiControl.step.Subscribe(OnNextStep);
        }
        

        private void OnNextStep(UIControl.Step step)
        {
            if (step != UIControl.Step.InGame)
                return;
            
            StartGame();
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