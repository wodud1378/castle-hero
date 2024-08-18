using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.Network.Service;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Stage.UI
{
    public class UIPrepare : UIMain
    {
        [SerializeField] private UIConfigCharacterList _characterList;
        [SerializeField] private UIConfigDragField _dragField;

        [SerializeField] private Button _speedUp;
        [SerializeField] private Button _back;

        public void Init()
        {
            this.SubscribeButton(_back, BackToLobby);
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

            Start();
        }

        private void BackToLobby() => Context.Transition.CurrentState = State.Lobby;

        private async void Start()
        {
            var entrance = Storage.userRepository.gameEntrance.Value;
            var canEntrance = await NetworkService.Game.Start(entrance.type, entrance.id);
            if (!canEntrance)
                return;

            Context.Transition.CurrentState = State.InGame;
        }
    }
}