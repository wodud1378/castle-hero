using System;
using RGLabs.InGame.Data;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UILobby : MonoBehaviour
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
                //.Skip(1)
                .Subscribe(OnNextStep)
                .AddTo(this);

            NextStepButton
                .OnClickAsObservable()
                .Subscribe(_ => NextStep())
                .AddTo(this);

            GoToLobbyButton
                .OnClickAsObservable()
                .Subscribe(_ => GoToLobby())
                .AddTo(this);
            
            _startConfig
                .OnClickAsObservable()
                .Subscribe(_ => StartConfig())
                .AddTo(this);

            _endConfig
                .OnClickAsObservable()
                .Subscribe(_ => EndConfig())
                .AddTo(this);
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
    }
}