using RGLabs.Common.Behaviours;
using RGLabs.Common.Pattern;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Lobby.UI;
using RGLabs.Unit.Factory;
using UniRx;
using UnityEngine;

namespace RGLabs.Lobby.Behaviours
{
    public struct StartGame
    {
        public DBCollections db;
        
        public UserRepository userRepo;
        public InGameRepository gameRepo;
        
        public PoolContainer poolContainer;
        public IUnitFactory unitFactory;
    }
    
    public class LobbyBehaviour : SceneBehaviour
    {
        [SerializeField] private UILobby uiLobby;

        [SerializeField] private Formation _formation;
        [SerializeField] private WaveRunner _waveRunner;
        [SerializeField] private SpriteRenderer _map;

        private async void Awake()
        {
            await Storage.InitAsync();
            
            gameRepo = Storage.inGameRepository;
            userRepo = Storage.userRepository;
            db = Storage.DB;
            poolContainer = new PoolContainer();
            unitFactory = new DefaultUnitFactory(poolContainer);

            await _formation.Init(db.characters, userRepo, gameRepo, unitFactory);
            
            uiLobby.Init();
            uiLobby.step.Subscribe(OnNextStep);
        }
        

        private void OnNextStep(UILobby.Step step)
        {
            if (step != UILobby.Step.InGame)
                return;
            
            StartGame();
        }
        
        private void StartGame()
        {
            _waveRunner.Init(unitFactory, db.monsters, db.waves, gameRepo.castle.Value);
            _waveRunner.isRunning = true;

            MessageBroker.Default.Publish(new StartGame
            {
                db = db,
                userRepo = userRepo,
                gameRepo = gameRepo,
                poolContainer = poolContainer,
                unitFactory = unitFactory
            });
        }
    }
}