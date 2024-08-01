using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Stage.UI
{
    public class UIStage : UIMain
    {
        [SerializeField] private UIConfigCharacterList _characterList;
        [SerializeField] private UIConfigDragField _dragField;

        [SerializeField] private Button _speedUp;
        [SerializeField] private Button _startConfig;
        [SerializeField] private Button _back;
        
        public void Init()
        {
            this.SubscribeButton(_startConfig, StartConfig);
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

        private async void StartConfig()
        {
            await _characterList.Init(Storage.userRepository.characters);
            
            _characterList.Open();
        }
        
        private void BackToLobby() => Context.Transition.CurrentState = State.Lobby;
    }
}