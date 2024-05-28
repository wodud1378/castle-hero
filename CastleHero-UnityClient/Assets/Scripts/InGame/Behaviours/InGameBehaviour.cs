using System;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.InGame.System;
using RGLabs.InGame.UI;
using RGLabs.Lobby.Behaviours;
using RGLabs.Lobby.UI;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Unit.Factory;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

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
            Context.currentBehaviour = this;

            var last = startGame.from;

            db = last.db;
            poolContainer = last.poolContainer;
            userRepo = last.userRepo;
            gameRepo = last.gameRepo;

            characterFactory = last.characterFactory;
            monsterFactory = last.monsterFactory;

            var castle = gameRepo.castle.Value;
            castle.state
                .Where(x => x == UnitCore.States.Dead)
                .Subscribe(_ => OnCastleDestroy())
                .AddTo(this);

            Context.startButton.enabled = false;
            
            _uiInGame.Init(poolContainer, gameRepo, db.units);
            _uiInGame.Open();
            
            _unitProcessor = new UnitProcessor(this);

            RunWave();
            RunUnits();
        }

        private void RunWave()
        {
            if (!db.stages.TryFind(userRepo.stage.Value, out var entity))
                return;

            var waves = db.waves.Map(entity.waveGroupId);
            _waveRunner.Init(waves, db.units, gameRepo.castle.Value, monsterFactory);
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
            foreach (var character in gameRepo.characters.Value)
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
                int currentStage = userRepo.stage.Value;
                if (exitCode == ExitCode.Retry)
                {
                    stage = currentStage;
                }
                else
                {
                    if (!db.stages.TryFindIndex(currentStage, out int index))
                        return;

                    if (!db.stages.TryIndexOf(index + 1, out var entity))
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