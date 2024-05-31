using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.InGame.System;
using RGLabs.InGame.UI;
using RGLabs.Lobby.Behaviours;
using RGLabs.Unit.Components;
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

    public struct Result
    {
        public bool isCleared;
    }

    public class InGameBehaviour : SceneBehaviour
    {
        [SerializeField] private UIInGame _uiInGame;
        [SerializeField] private WaveRunner _waveRunner;

        private UnitProcessor _unitProcessor;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            
            this.SubscribeMessage<StartGame>(Run);
            this.SubscribeMessage<ExitCode>(Exit);
        }

        private void Run(StartGame startGame)
        {
            var castle = Storage.inGameRepository.castle.Value;
            castle.state
                .Where(x => x == UnitCore.States.Dead)
                .Subscribe(_ => OnCastleDestroy())
                .AddTo(this);

            Context.startButton.enabled = false;
            
            _uiInGame.Init();
            _uiInGame.Open();
            
            _unitProcessor = new UnitProcessor(this);

            RunWave();
            RunUnits();
        }

        private void RunWave()
        {
            var db = Storage.db;
            var stage = Storage.userRepository.stage.Value;
            if (!db.stages.TryFind(stage, out var entity))
                return;

            _waveRunner.Init(entity.waveGroupId);
            _waveRunner.isRunning = true;
            _waveRunner.completed
                .Where(x => x)
                .Subscribe(_ => OnWaveComplete());
        }

        private void OnCastleDestroy() => SetResult(false);

        private void OnWaveComplete() => SetResult(true);

        private void SetResult(bool isCleared)
        {
            _waveRunner.isRunning = false;

            new Result { isCleared = isCleared }.Publish();
        }

        private void RunUnits()
        {
            foreach (var character in Storage.inGameRepository.characters)
            {
                character.Core.inBattle = true;
                character.CanAttack = true;
                character.CanMove = true;
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
                int currentStage = Storage.userRepository.stage.Value;
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
            
            _uiInGame.Dispose();
        }
    }
}