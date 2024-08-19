using System;
using RGLabs.Castle;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.Lobby.Shop.UI;
using RGLabs.Lobby.UI;
using RGLabs.Prepare.UI;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace RGLabs.Lobby.Behaviours
{
    public struct StartGame
    {
        public IGameEntity entity;
    }

    public class LobbyBehaviour : SceneBehaviour, IBackButtonListener
    {
        [SerializeField] private UILobby _uiLobby;
        [FormerlySerializedAs("_uiStage")] [SerializeField] private UIPrepare uiPrepare;
        [SerializeField] private UIShop _uiShop;
        [SerializeField] private UICastle _uiCastle;

        [SerializeField] private FormationField _formation;
        [SerializeField] private SpriteRenderer _map;

        private UIMain _current;

        protected override async void OnLoaded()
        {
            base.OnLoaded();

            await _formation.Init();

            uiPrepare.Init();

            Context.Transition.StateObserver
                .DistinctUntilChanged()
                .Subscribe(OnNextState)
                .AddTo(this);
        }

        private void OnNextState(State state)
        {
            switch (state)
            {
                case State.Lobby:
                    TransitionTo(_current, _uiLobby);
                    Context.startButton.enabled = true;
                    Context.Back.Remove(this);
                    break;
                case State.Shop:
                    TransitionTo(_current, _uiShop, ()=> _uiShop.Init());
                    Context.startButton.enabled = false;
                    Context.Back.Add(this);
                    break;
                case State.Prepare:
                    TransitionTo(_current, uiPrepare);
                    Context.startButton.enabled = false;
                    Context.Back.Add(this);
                    break;
                case State.Castle:
                    TransitionTo(_current, _uiCastle);
                    Context.startButton.enabled = false;
                    Context.Back.Add(this);
                    break;
                case State.InGame:
                    TransitionTo(_current, null, StartGame);
                    break;
            }
        }

        private void TransitionTo(UIMain from, UIMain to = null, Action onTransitionEnd = null)
        {
            if (from != null)
            {
                if (!from.IsOpen)
                    OnTransitionEnd(to, onTransitionEnd);
                else
                    from.OnCloseAnimationEnd += () => OnTransitionEnd(to, onTransitionEnd);

                SetMainUIActive(from, false);
            }
            else
            {
                OnTransitionEnd(to, onTransitionEnd);
            }

            _current = to;
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
            var entrance = Storage.userRepository.entrance.Value;
            if (!Storage.db.TryLoadGameEntity(entrance.type, entrance.id, out var entity))
                return;
            
            _uiLobby.Dispose();
            uiPrepare.Dispose();
            
            new StartGame { entity = entity }.Publish();

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
            if (state != State.Prepare)
                return false;

            Context.Transition.CurrentState = State.Lobby;
            return true;
        }
    }
}