using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Lobby.UI;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Stage.UI
{
    public class UIStage : UIMain
    {
        [SerializeField] private UIConfigCharacterList _characterList;
        
        [SerializeField] private Button _startConfig;
        [SerializeField] private Button _back;
        
        public void Init()
        {
            this.SubscribeButton(_startConfig, StartConfig);
            this.SubscribeButton(_back, BackToLobby);
        }

        private async void StartConfig()
        {
            await _characterList.Init(Storage.userRepository.characters);
            
            _characterList.Open();
        }
        
        private void BackToLobby() => Context.Transition.CurrentState = State.Lobby;
    }
}