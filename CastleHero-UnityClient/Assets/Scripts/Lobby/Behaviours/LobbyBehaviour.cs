using System;
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
            TransitionTo(state);

            if (state == State.InGame)
                StartGame();
        }

        private void TransitionTo(State state)
        {
            if (state == State.InGame)
            {
                _uiLobby.Close();
                _uiStage.Close();
            }

            UIMain from;
            UIMain to;
            StartButton.Mode mode;
            if (state == State.Lobby)
            {
                from = _uiStage;
                to = _uiLobby;
                mode = StartButton.Mode.Lobby;
            }
            else
            {
                from = _uiLobby;
                to = _uiStage;
                mode = StartButton.Mode.Stage;
            }

            string lockKey = "LobbyTransition";
            Context.uiLock.Set(lockKey);
            
            Action onClosed = () =>
            {
                Context.uiLock.Release(lockKey);
                SetMainUIActive(to, true);
            };

            if (!from.IsOpen)
                onClosed.Invoke();
            else
                from.OnCloseAnimationEnd += onClosed;

            SetMainUIActive(from, false);
            
            Context.startButton.mode.Value = mode;
        }

        private void SetMainUIActive(UIMain ui, bool isActive)
        {
            if (isActive)
            {
                if (!ui.IsOpen)
                    ui.Open();
            }
            else if (ui.IsOpen)
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