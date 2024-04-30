using RGLabs.Common.Behaviours;
using RGLabs.InGame.System;
using RGLabs.InGame.UI;
using RGLabs.Lobby.Behaviours;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public class InGameBehaviour : SceneBehaviour
    {
        [SerializeField] private UIInGame _uiInGame;
        [SerializeField] private WaveRunner _waveRunner;
        
        private void Awake()
        {
            MessageBroker.Default
                .Receive<StartGame>()
                .Subscribe(Run);
        }

        private async void Run(StartGame startGame)
        {
            db = startGame.db;
            userRepo = startGame.userRepo;
            gameRepo = startGame.gameRepo;
            unitFactory = startGame.unitFactory;
            poolContainer = startGame.poolContainer;

            await _uiInGame.InitAsync();
            
            var processor = new UnitProcessor();
            
            RunWave();
            RunUnits();
        }

        private void RunWave()
        {
            if (!db.stages.TryFind(userRepo.stage.Value, out var entity))
                return;

            var waves = db.waves.Map(entity.waveGroupId);
            _waveRunner.Init(waves, db.monsters, gameRepo.castle.Value, unitFactory);
            _waveRunner.isRunning = true;

            _waveRunner.completed
                .Where(x => x)
                .Subscribe(_ => OnWaveComplete());
        }

        private void OnWaveComplete()
        {
            
        }
        
        private void RunUnits()
        {
            foreach (var character in gameRepo.characters.Value)
            {
                var unit = character.behaviour;
                unit.canAttack = true;
                unit.canMove = true;
            }
        }
    }
}