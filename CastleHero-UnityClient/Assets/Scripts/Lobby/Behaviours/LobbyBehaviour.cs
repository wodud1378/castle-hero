using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Common.Pattern;
using RGLabs.Lobby.UI;
using RGLabs.Stage.UI;
using RGLabs.Unit.Factory;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.Lobby.Behaviours
{
    public struct StartGame
    {
        public PoolContainer poolContainer;
        public IUnitFactory unitFactory;
    }
    
    public class LobbyBehaviour : SceneBehaviour
    {
        [SerializeField] private UILobby _uiLobby;
        [SerializeField] private UIStage _uiStage;

        [SerializeField] private Formation _formation;
        [SerializeField] private SpriteRenderer _map;

        protected override async void OnLoaded()
        {
            base.OnLoaded();
            
            poolContainer = new PoolContainer();
            unitFactory = new UnitFactory(poolContainer);
            
            await _formation.Init(db.characters, userRepo, gameRepo, unitFactory);
            
            _uiStage.Init();
            Context.Transition.StateObserver
                .DistinctUntilChanged()
                .Subscribe(OnNextState)
                .AddTo(this);
        }

        private void OnNextState(State state)
        {
            var mode = state switch
            {
                State.Lobby => StartButton.Mode.Lobby,
                State.Stage => StartButton.Mode.Stage,
                _=> Context.startButton.mode.Value
            };

            Context.startButton.mode.Value = mode;

            SetMainUIActive(_uiLobby, state == State.Lobby);
            SetMainUIActive(_uiStage, state == State.Stage);

            if(state == State.InGame)
                StartGame();
        }

        private void SetMainUIActive(UIMain ui, bool isActive)
        {
            if (isActive)
            {
                if(!ui.IsOpen)
                    ui.Open();
            }
            else if(ui.IsOpen)
                ui.Close();
        }

        private void StartGame()
        {
            new StartGame
            {
                poolContainer = poolContainer,
                unitFactory = unitFactory
            }.Publish();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            _uiLobby.Dispose();
        }
    }
}