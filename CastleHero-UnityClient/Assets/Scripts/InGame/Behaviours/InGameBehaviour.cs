using RGLabs.Common.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Lobby.Behaviours;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public class InGameBehaviour : SceneBehaviour
    {
        [SerializeField] private WaveRunner _waveRunner;
        
        private void Awake()
        {
            MessageBroker.Default
                .Receive<StartGame>()
                .Subscribe(Run);
        }

        private void Run(StartGame startGame)
        {
            db = startGame.db;
            userRepo = startGame.userRepo;
            gameRepo = startGame.gameRepo;
            unitFactory = startGame.unitFactory;
            poolContainer = startGame.poolContainer;

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
        }
        
        private void RunUnits()
        {
            foreach (var unit in gameRepo.units.Value)
            {
                unit.canAttack = true;
                unit.canMove = true;
            }
        }
    }
}