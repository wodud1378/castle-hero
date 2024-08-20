using System.Linq;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.Network.Service;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Prepare.UI
{
    public class UIPrepare : UIMain
    {
        [SerializeField] private UIConfigCharacterList _characterList;
        [SerializeField] private UIConfigDragField _dragField;

        [SerializeField] private Button _speedUp;

        public void Init()
        {
            this.SubscribeButton(_speedUp, () =>
            {
                var repository = Storage.inGameRepository;
                repository.speedUp = !repository.speedUp;
            });

            _characterList.isOpened
                .Subscribe(x => _dragField.gameObject.SetActive(x))
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

            _characterList.Close();
        }

        protected override void OnBack() => BackToLobby();

        private async void StartAfterConfig()
        {
            await _characterList.Init(Storage.userRepository.characters.units);

            _characterList.Open();

            var canceled = await _characterList
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
            var repository = Storage.userRepository;
            var prop = repository.entrance;
            if (prop.Value.type == GameType.Dungeon)
            {
                prop.Value = new GameEntrance
                {
                    type = GameType.Stage,
                    id = repository.StageFocus
                };
            }
            
            Context.Transition.CurrentState = State.Lobby;
        }

        private async void StartGame()
        {
            var entrance = Storage.userRepository.entrance.Value;
            var canEntrance = await NetworkService.Game.Start(entrance.type, entrance.id);
            if (!canEntrance)
                return;

            Context.Transition.CurrentState = State.InGame;
        }
    }
}