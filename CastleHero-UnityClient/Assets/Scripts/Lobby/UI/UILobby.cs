using System;
using System.Collections.Generic;
using RGLabs.Data;
using RGLabs.InGame.UI;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UILobby : MonoBehaviour, IDisposable
    {
        public enum Step
        {
            Lobby,
            Stage,
            InGame,
        }

        [Serializable]
        public struct TriggerSet
        {
            public Step step;
            public string trigger;
            public Animator animator;
        }

        [field: SerializeField] public Button NextStepButton { get; private set; }
        [field:SerializeField] public Button GoToLobbyButton { get; private set; }
        [field: SerializeField] public UIStageSelect StageSelect { get; private set; }
        [field: SerializeField] public UIConfigFormation ConfigFormation { get; private set; }
        [field: SerializeField] public UICharacterList CharacterList { get; private set; }

        [SerializeField] private Button _startConfig;
        [SerializeField] private Button _endConfig;
        
        [SerializeField] private TriggerSet[] _triggerSets;

        public readonly ReactiveProperty<Step> step = new(Step.Lobby);
        
        public void Init()
        {
            InitUI();
            InitSubscriptions();
        }

        private void InitUI()
        {
            StageSelect.Init();
            ConfigFormation.Init();
        }

        private void InitSubscriptions()
        {
            step
                .DistinctUntilChanged()
                .Subscribe(OnNextStep)
                .AddTo(this);

            this.SubscribeButton(NextStepButton, NextStep);
            this.SubscribeButton(GoToLobbyButton, GoToLobby);
            this.SubscribeButton(_startConfig, StartConfig);
            this.SubscribeButton(_endConfig, EndConfig);
        }

        private void GoToLobby()
        {
            if (step.Value == Step.Lobby)
                return;

            step.Value = Step.Lobby;
        }
        
        private void NextStep()
        {
            if (step.Value == Step.InGame)
                return;

            ++step.Value;
        }
        
        private async void StartConfig()
        {
            await CharacterList.Init(Storage.userRepository.characters.Value, Storage.DB.characters);
            
            CharacterList.Open();
            ConfigFormation.enabled = true;

            _startConfig.enabled = false;
            _endConfig.enabled = true;
        }

        private void EndConfig()
        {
            CharacterList.Close();
            CharacterList.Dispose();
            ConfigFormation.enabled = false;

            _startConfig.enabled = true;
            _endConfig.enabled = false;
        }

        private void OnNextStep(Step step)
        {
            foreach (var set in _triggerSets)
            {
                if (set.step != step)
                    continue;

                set.animator.SetTrigger(set.trigger);
            }

            bool isLobby = step == Step.Lobby;
            StageSelect.enabled = isLobby;
            
            if(isLobby)
                StageSelect.Set(Storage.userRepository, Storage.DB);
            
            bool isStageStep = step == Step.Stage;
            _startConfig.gameObject.SetActive(isStageStep);
            _endConfig.gameObject.SetActive(isStageStep);
        }

        public void Dispose()
        {
            step.Dispose();
            CharacterList.Dispose();
            StageSelect.Dispose();
            ConfigFormation.Dispose();
        }
    }
}