using CastleHero.View.Common;
using CastleHero.Common.Flow;
using CastleHero.Common.Pattern;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data.Repositories;
using CastleHero.Data.UseCases;
using CastleHero.View.Lobby.Prepare.Actions;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using Cysharp.Threading.Tasks;
namespace CastleHero.View.Lobby.Prepare.UI
{
    public class UIPrepare : UIMain
    {
        [FormerlySerializedAs("_characterList")]
        [SerializeField] private UIConfigCharacterList characterList;
        [FormerlySerializedAs("_dragField")]
        [SerializeField] private UIConfigDragField dragField;

        [FormerlySerializedAs("_speedUp")]
        [SerializeField] private Button speedUp;

        private ISettingRepository _settings;
        private IUserRepository _userRepo;
        private IPopupManager _popups;
        private StateManager<LobbyState> _lobbyState;
        private StateManager<State> _globalState;

        private GameStartAction _gameStartAction;
        private EntranceManager _entranceManager;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _settings = sl.Get<ISettingRepository>();
            _userRepo = sl.Get<IUserRepository>();
            _popups = sl.Get<IPopupManager>();
            _lobbyState = sl.Get<StateManager<LobbyState>>();
            _globalState = sl.Get<StateManager<State>>();

            _gameStartAction = sl.Get<GameStartAction>();
            _entranceManager = sl.Get<EntranceManager>();
        }

        public void Init()
        {
            this.SubscribeButton(speedUp, () =>
            {
                _settings.speedUp.Value = !_settings.speedUp.Value;
            });

            characterList.isOpened
                .Subscribe(x => dragField.gameObject.SetActive(x))
                .AddTo(this);
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            StartAfterConfig();
        }

        protected override void OnClose()
        {
            base.OnClose();

            characterList.Close();
        }

        protected override void OnBack() => BackToLobby();

        private async UniTask StartAfterConfig()
        {
            characterList.Init(_userRepo.Characters.Units);

            characterList.Open();

            var canceled = await characterList
                .ConfigTask
                .SuppressCancellationThrow();

            if (canceled)
            {
                BackToLobby();
                return;
            }

            StartGame();
        }

        private void BackToLobby()
        {
            _entranceManager.RevertToStageIfDungeon();
            _lobbyState.CurrentState = LobbyState.Main;
        }

        private async UniTask StartGame()
        {
            var result = await _gameStartAction.RequestStart();
            if (!result.IsSuccess)
            {
                _popups.Open<PopupCommon>(result.error);
                return;
            }

            _globalState.CurrentState = State.InGame;
        }
    }
}
