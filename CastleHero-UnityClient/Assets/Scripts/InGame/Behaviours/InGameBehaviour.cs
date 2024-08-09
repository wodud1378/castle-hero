using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.InGame.System;
using RGLabs.InGame.UI;
using RGLabs.Lobby.Behaviours;
using RGLabs.Network.Service;
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

        private StageTimer _timer;
        private UnitProcessor _unitProcessor;
        private InGameRepository _repository;
  
        protected override void OnAwake()
        {
            base.OnAwake();
            
            this.SubscribeMessage<ExitCode>(Exit);
            this.SubscribeMessage<StartGame>(Run);
            this.SubscribeMessage<TimeOver>(_=> OnStageFailed());
        }

        private void Run(StartGame startGame)
        {
            if (!Storage.db.stages.TryFind(startGame.stage, out var stageData))
                return;
            
            Time.timeScale = Storage.inGameRepository.speedUp
                ? 2f
                : 1f;
            
            var cam = Camera.main;
            cam.DOOrthoSize(20f, 0.4f);
            
            _uiInGame.gameObject.SetActive(true);

            _repository = Storage.inGameRepository;
            _repository.stage = stageData.Id;
            _repository.leftTime.Value = stageData.timeLimit;
            
            var castle = _repository.castle.Value;
            castle.state
                .Where(x => x == UnitCore.States.Dead)
                .Subscribe(_ => OnStageFailed())
                .AddTo(this);

            InitGlobalSkills();
            
            Context.startButton.enabled = false;
            
            _uiInGame.Init();
            _uiInGame.Open();
            
            _unitProcessor = new ();
            _timer = new();

            RunWave();
            RunUnits();
        }

        private void InitGlobalSkills()
        {
            int lv = Storage.userRepository.profile.castleLv.Value;
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
            var stage = _repository.stage;
            if (!db.stages.TryFind(stage, out var entity))
                return;

            _waveRunner.Init(entity.waveGroupId);
            _waveRunner.isRunning = true;
            _waveRunner.completed
                .Where(x => x)
                .Subscribe(_ => OnWaveComplete());
        }

        private void OnStageFailed()
        {
            new GameResult
            {
                isCleared = false,
                data = FailedData()
            }.Publish();
        }

        private StageCleared FailedData()
        {
            var transitions = new List<UnitTransition>();
            var repository = Storage.userRepository;
            var units = repository.characters.units
                .Where(unit => repository.formation.fieldUnits.FirstOrDefault(x => x.id == unit.id) != null);

            foreach (var unit in units)
            {
                int rate = unit.rate;
                int lv = unit.lv;
                int exp = unit.exp;
                transitions.Add(new UnitTransition
                {
                    rateTransition = new[] { rate, rate },
                    lvTransition = new[] { lv, lv },
                    expTransition = new[] { exp, exp },
                    unit = unit
                });
            }

            return new StageCleared
            {
                stage = Storage.inGameRepository.stage,
                exp = 0,
                isFirstClear = false,
                currency = null,
                items = new List<IItem>(),
                transitions = transitions
            };
        }

        private async void OnWaveComplete()
        {
            var result = await NetworkService.Stage.SetClear(Storage.userRepository.profile.focusedStage.Value);
            new GameResult
            {
                isCleared = true,
                data = result
            }.Publish();
        }

        private void RunUnits()
        {
            foreach (var character in _repository.characters)
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
                int currentStage = _repository.stage;
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
            _timer.Dispose();
        }
    }
}