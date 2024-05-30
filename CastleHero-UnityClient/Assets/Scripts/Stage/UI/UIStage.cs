using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.InGame.UI;
using RGLabs.Lobby.UI;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Stage.UI
{
    public class UIStage : UIMain
    {
        [SerializeField] private UIConfigFormation _configFormation;
        [SerializeField] private UICharacterList _characterList;
        
        [SerializeField] private Button _startConfig;
        [SerializeField] private Button _endConfig;
        [SerializeField] private Button _back;

        public void Init()
        {
            _configFormation.Init();
            
            this.SubscribeButton(_startConfig, StartConfig);
            this.SubscribeButton(_endConfig, EndConfig);
            this.SubscribeButton(_back, BackToLobby);
        }

        private async void StartConfig()
        {
            await _characterList.Init(Storage.userRepository.characters);
            
            _characterList.Open();
            _configFormation.enabled = true;

            _startConfig.enabled = false;
            _endConfig.enabled = true;
        }

        private void EndConfig()
        {
            _characterList.Close();
            _characterList.Dispose();
            _configFormation.enabled = false;

            _startConfig.enabled = true;
            _endConfig.enabled = false;
        }

        private void BackToLobby() => Context.Transition.CurrentState = State.Lobby;
    }
}