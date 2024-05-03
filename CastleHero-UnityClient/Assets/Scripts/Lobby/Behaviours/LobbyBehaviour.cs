using RGLabs.Common.Behaviours;
using RGLabs.Common.Pattern;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.Lobby.UI;
using RGLabs.Unit.Factory;
using RGLabs.Utility;
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
        [SerializeField] private SpriteRenderer _map;

        protected override async void OnAwake()
        {
            base.OnAwake();
            
            activated.Add(this);
            
            await Storage.InitAsync();
            
            gameRepo = Storage.inGameRepository;
            userRepo = Storage.userRepository;
            db = Storage.DB;
            poolContainer = new PoolContainer();
            unitFactory = new UnitFactory(poolContainer);

            await _formation.Init(db.characters, userRepo, gameRepo, unitFactory);
           
            uiLobby.Init();
            uiLobby.step
                .Subscribe(OnNextStep)
                .AddTo(this);

            var step = Storage.StartUpData.step;
            if (step == UILobby.Step.InGame)
            {
                userRepo.stage.Value = Storage.StartUpData.stage;
                uiLobby.step.Value = step;
            }
        }

        private void OnNextStep(UILobby.Step step)
        {
            if (step != UILobby.Step.InGame)
                return;
            
            StartGame();
        }
        
        private void StartGame()
        {
            new StartGame
            {
                db = db,
                userRepo = userRepo,
                gameRepo = gameRepo,
                poolContainer = poolContainer,
                unitFactory = unitFactory
            }.Publish();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            uiLobby.Dispose();
        }
    }
}