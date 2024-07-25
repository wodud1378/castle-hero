using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.InGame.System;
using RGLabs.InGame.UI;
using RGLabs.Lobby.Behaviours;
using RGLabs.Network.Service.Stage;
using RGLabs.Network.Shared;
using RGLabs.Unit.Components;
using RGLabs.Unit.Skill.Global;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    public enum ExitCode
    {
        Exit,
        Retry,
        Next,
    }

    public struct GameResult
    {
        public bool isCleared;
        public StageCleared data;
    }

    public class InGameBehaviour : SceneBehaviour
    {
        [SerializeField] private UIInGame _uiInGame;
        [SerializeField] private WaveRunner _waveRunner;

        private readonly StageService _service = new();
        private UnitProcessor _unitProcessor;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            
            this.SubscribeMessage<StartGame>(Run);
            this.SubscribeMessage<ExitCode>(Exit);
        }

        private void Run(StartGame startGame)
        {
            var cam = Camera.main;
            cam.DOOrthoSize(20f, 0.4f);
            
            _uiInGame.gameObject.SetActive(true);
            
            var castle = Storage.inGameRepository.castle.Value;
            castle.state
                .Where(x => x == UnitCore.States.Dead)
                .Subscribe(_ => OnCastleDestroy())
                .AddTo(this);

            InitGlobalSkills();
            
            Context.startButton.enabled = false;
            
            _uiInGame.Init();
            _uiInGame.Open();
            
            _unitProcessor = new UnitProcessor();

            RunWave();
            RunUnits();
        }

        private void InitGlobalSkills()
        {
            int lv = Storage.userRepository.castleLv.Value;
            if (!Storage.db.castles.TryFind(lv, out var entity))
                return;

            int index = 0;
            var list = new List<GlobalSkill.Parameter>();
            while (index.IsValidIndex(entity.skills, entity.skillValues))
            {
                var type = Enum.Parse<GlobalSkill.Type>(entity.skills[index]);
                if (type == GlobalSkill.Type.Damage)
                {
                    list.Add(new GlobalSkill.Parameter
                    {
                        type = type,
                        radius = 5f,
                        value = entity.skillValues[index],
                        coolTime = 7.5f,
                        centerEffect = Constants.GlobalSkillEffect[type]
                    });
                }
                ++index;
            }

            _uiInGame.GlobalSkill.Init(list).Forget();
        }

        private void RunWave()
        {
            var db = Storage.db;
            var stage = Storage.userRepository.focusedStage.Value;
            if (!db.stages.TryFind(stage, out var entity))
                return;

            _waveRunner.Init(entity.waveGroupId);
            _waveRunner.isRunning = true;
            _waveRunner.completed
                .Where(x => x)
                .Subscribe(_ => OnWaveComplete());
        }

        private void OnCastleDestroy()
        {
            new GameResult
            {
                isCleared =  false
            }.Publish();
        }

        private async void OnWaveComplete()
        {
            var result = await _service.SetClear(Storage.userRepository.focusedStage.Value);
            
            new GameResult
            {
                isCleared = true,
                data = result
            }.Publish();
        }

        private void RunUnits()
        {
            foreach (var character in Storage.inGameRepository.characters)
            {
                character.Core.onRest.Value = false;
            }
        }

        private void Exit(ExitCode exitCode)
        {
            _waveRunner.isRunning = false;
            
            int stage = 0;
            State state;
            if (exitCode != ExitCode.Exit)
            {
                state = State.InGame;
                int currentStage = Storage.userRepository.focusedStage.Value;
                if (exitCode == ExitCode.Retry)
                {
                    stage = currentStage;
                }
                else
                {
                    var db = Storage.db.stages;
                    if (!db.TryFindIndex(currentStage, out int index))
                        return;

                    if (!db.TryIndexOf(index + 1, out var entity))
                        return;

                    stage = entity.Id;
                }
            }
            else
            {
                state = State.Lobby;
            }

            Storage.entranceData = new Entrance
            {
                state = state,
                stage = stage
            };
            
            LoadSceneAfterDispose("Main");
        }

        public override void Dispose()
        {
            base.Dispose();
            
            _unitProcessor.Dispose();
            _uiInGame.Dispose();
        }
    }
}