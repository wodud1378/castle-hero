using System.Linq;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.Common.Pattern;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;
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

        public void Init()
        {
            this.SubscribeButton(speedUp, () =>
            {
                var setting = ServiceLocator.Get<CastleHero.Data.Repositories.SettingRepository>();
                setting.speedUp.Value = !setting.speedUp.Value;
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
            await characterList.Init(ServiceLocator.Get<CastleHero.Data.Repositories.IUserRepository>().Characters.Units);

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
            var repository = ServiceLocator.Get<CastleHero.Data.Repositories.IUserRepository>();
            var prop = repository.Entrance;
            if (prop.Value.type == GameType.Dungeon)
            {
                prop.Value = new GameEntrance
                {
                    type = GameType.Stage,
                    id = repository.StageFocus
                };
            }

            ServiceLocator.Get<CastleHero.Common.Flow.StateManager<CastleHero.Common.Flow.LobbyState>>().CurrentState = LobbyState.Main;
        }

        private async UniTask StartGame()
        {
            var entrance = ServiceLocator.Get<CastleHero.Data.Repositories.IUserRepository>().Entrance.Value;
            var result = await ServiceLocator.Get<INetworkServiceProvider>().Game.Start(entrance.type, entrance.id);
            if (!result.IsSuccess)
            {
                ServiceLocator.Get<CastleHero.View.Common.IPopupManager>().Open<PopupCommon>(result.error);
                return;
            }
            if (!result.IsSuccess)
                return;

            ServiceLocator.Get<CastleHero.Common.Flow.StateManager<CastleHero.Common.Flow.State>>().CurrentState = State.InGame;
        }
    }
}
