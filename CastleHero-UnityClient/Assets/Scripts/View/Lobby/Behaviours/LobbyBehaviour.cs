using System;
using Cysharp.Threading.Tasks;
using CastleHero.View.Castle;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.Common.Pattern;
using CastleHero.Common.ResourceManagement;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.View.Lobby.Actions;
using CastleHero.View.Lobby.Shop.UI;
using CastleHero.View.Lobby.UI;
using CastleHero.View.Lobby.UI.Popup;
using CastleHero.View.Lobby.Prepare.UI;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.Behaviours
{
    public class LobbyScene : SceneBase, IBackButtonListener
    {
        [FormerlySerializedAs("_bgm")]
        [SerializeField] private AssetReference bgm;

        [FormerlySerializedAs("_uiLobby")]
        [SerializeField] private UILobby uiLobby;
        [FormerlySerializedAs("_uiPrepare")]
        [SerializeField] private UIPrepare uiPrepare;
        [FormerlySerializedAs("_uiShop")]
        [SerializeField] private UIShop uiShop;
        [FormerlySerializedAs("_uiCastle")]
        [SerializeField] private UICastle uiCastle;

        [FormerlySerializedAs("_formation")]
        [SerializeField] private FormationField formation;

        // 의존성은 OnLoaded 진입 시 ServiceLocator 에서 한 번 캐싱. 이후 모든 메서드는 필드 참조.
        private StateManager<LobbyState> _lobbyState;
        private StateManager<State> _state;
        private IUserRepository _userRepo;
        private IDBProvider _db;
        private IPopupManager _popups;
        private StartButton _startButton;
        private BackButton _back;

        private LobbyAction _presenter;

        private UIMain _current;

        protected override async UniTask OnLoaded()
        {
            base.OnLoaded();

            var sl = ServiceLocator.Instance;
            _lobbyState = sl.Get<StateManager<LobbyState>>();
            _state = sl.Get<StateManager<State>>();
            _userRepo = sl.Get<IUserRepository>();
            _db = sl.Get<IDBProvider>();
            _popups = sl.Get<IPopupManager>();
            _startButton = sl.Get<StartButton>();
            _back = sl.Get<BackButton>();

            _presenter = sl.Get<LobbyAction>();

            await formation.Init();

            uiPrepare.Init();

            UpdateStamina();

            _lobbyState.StateObserver
                .DistinctUntilChanged()
                .Subscribe(OnNextState)
                .AddTo(this);

            _state.StateObserver
                .DistinctUntilChanged()
                .Where(x => x == State.InGame)
                .Subscribe(_ => TransitionTo(_current, null, StartGame))
                .AddTo(this);

            ReceiveSubscribeProducts();
        }

        private async UniTask UpdateStamina()
        {
            var result = await _presenter.UpdateStamina();

            if (!result.IsSuccess)
                _popups.Open<PopupCommon>(result.error);
        }

        private async UniTask ReceiveSubscribeProducts()
        {
            var result = await _presenter.TryReceiveSubscribedItems();
            if (result == null)
                return;

            if (!result.IsSuccess)
                _popups.Open<PopupCommon>(result.error);
            else
                _popups.Open<PopupReceivedItems>(result.data);
        }

        private void OnNextState(LobbyState state)
        {
            switch (state)
            {
                case LobbyState.Main:
                    TransitionTo(_current, uiLobby);
                    _startButton.enabled = true;
                    _back.Remove(this);
                    break;
                case LobbyState.Shop:
                    TransitionTo(_current, uiShop, () => uiShop.Init());
                    _startButton.enabled = false;
                    _back.Add(this);
                    break;
                case LobbyState.Prepare:
                    TransitionTo(_current, uiPrepare);
                    _startButton.enabled = false;
                    _back.Add(this);
                    break;
                case LobbyState.Castle:
                    TransitionTo(_current, uiCastle);
                    _startButton.enabled = false;
                    _back.Add(this);
                    break;
            }
        }

        private void TransitionTo(UIMain from, UIMain to = null, Action onTransitionEnd = null)
        {
            if (from != null)
            {
                if (!from.IsOpen)
                    OnTransitionEnd(from, onTransitionEnd);
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
            var entrance = _userRepo.Entrance.Value;
            if (!_db.TryLoadGameEntity(entrance.type, entrance.id, out var entity))
                return;

            uiLobby.Dispose();
            uiPrepare.Dispose();

            new StartGame { entity = entity, draft = formation.Draft }.Publish();

            _back.Clear();
        }

        public override void Dispose()
        {
            base.Dispose();

            uiLobby.Dispose();
        }

        public bool OnProcessBack()
        {
            if (_lobbyState.CurrentState != LobbyState.Prepare)
                return false;

            _lobbyState.CurrentState = LobbyState.Main;
            return true;
        }
    }
}
