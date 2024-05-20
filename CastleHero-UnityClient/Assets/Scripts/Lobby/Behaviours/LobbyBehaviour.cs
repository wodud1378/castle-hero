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
        public SceneBehaviour from;
    }

    public class LobbyBehaviour : SceneBehaviour, IBackButtonListener
    {
        [SerializeField] private UILobby _uiLobby;
        [SerializeField] private UIStage _uiStage;

        [SerializeField] private Formation _formation;
        [SerializeField] private SpriteRenderer _map;

        protected override async void OnLoaded()
        {
            base.OnLoaded();

            poolContainer = new PoolContainer();
            monsterFactory = new UnitFactory(poolContainer, db.units, db.levels, db.skills);
            characterFactory = new UnitFactory(poolContainer, db.units, db.levels, db.skills);

            await _formation.Init(db.units, userRepo, gameRepo, characterFactory);

            _uiStage.Init();
            Context.currentBehaviour = this;
            Context.Transition.StateObserver
                .DistinctUntilChanged()
                .Subscribe(OnNextState)
                .AddTo(this);
        }

        private void OnNextState(State state)
        {
            if (state == State.InGame)
            {
                TransitionTo(_uiStage, null, StartGame);
                return;
            }

            if (state == State.Lobby)
            {
                TransitionTo(_uiStage, _uiLobby);

                Context.startButton.mode.Value = StartButton.Mode.Lobby;
                Context.Back.Remove(this);
            }
            else
            {
                TransitionTo(_uiLobby, _uiStage);

                Context.startButton.mode.Value = StartButton.Mode.Stage;
                Context.Back.Add(this);
            }
        }

        private void TransitionTo(UIMain from, UIMain to = null, Action onTransitionEnd = null)
        {
            if (!from.IsOpen)
                OnTransitionEnd(to, onTransitionEnd);
            else
                from.OnCloseAnimationEnd += () => OnTransitionEnd(to, onTransitionEnd);

            SetMainUIActive(from, false);
        }

        private void OnTransitionEnd(UIMain target, Action onTransitionEnd)
        {
            onTransitionEnd?.Invoke();

            if (target != null)
                SetMainUIActive(target, true);
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
            _uiLobby.Dispose();
            _uiStage.Dispose();

            Destroy(_uiLobby.gameObject);
            Destroy(_uiStage.gameObject);

            new StartGame
            {
                from = this
            }.Publish();

            Context.Back.Clear();
        }

        public override void Dispose()
        {
            base.Dispose();

            _uiLobby.Dispose();
        }

        public bool OnProcessBack()
        {
            var state = Context.Transition.CurrentState;
            if (state != State.Stage)
                return false;

            Context.Transition.CurrentState = State.Lobby;
            return true;
        }
    }
}