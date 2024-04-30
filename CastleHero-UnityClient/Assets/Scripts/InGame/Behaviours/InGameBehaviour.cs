using RGLabs.Common.Behaviours;
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
            
            RunWave();
            RunUnits();
        }

        private void RunWave()
        {
            _waveRunner.Init(unitFactory, db.monsters, db.waves, gameRepo.castle.Value);
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