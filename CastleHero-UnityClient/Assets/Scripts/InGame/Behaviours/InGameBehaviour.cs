using System;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.InGame.System;
using RGLabs.InGame.UI;
using RGLabs.Lobby.Behaviours;
using RGLabs.Lobby.UI;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RGLabs.InGame.Behaviours
{
    public enum ExitCode
    {
        Exit,
        Retry,
        Next,
    }

    public struct ExitRequest
    {
        public ExitCode code;
    }

    public struct Result
    {
        public bool isCleared;
    }

    public class InGameBehaviour : SceneBehaviour
    {
        [SerializeField] private UIInGame _uiInGame;
        [SerializeField] private WaveRunner _waveRunner;

        private void Awake()
        {
            activated.Add(this);
            
            MessageBroker.Default
                .Receive<StartGame>()
                .Subscribe(Run);

            MessageBroker.Default
                .Receive<ExitRequest>()
                .Subscribe(Exit);
        }

        private async void Run(StartGame startGame)
        {
            db = startGame.db;
            userRepo = startGame.userRepo;
            gameRepo = startGame.gameRepo;
            unitFactory = startGame.unitFactory;
            poolContainer = startGame.poolContainer;

            var castle = gameRepo.castle.Value;
            castle.state
                .Where(x => x == UnitBehaviour.States.Dead)
                .Subscribe(_ => OnCastleDestroy())
                .AddTo(this);

            await _uiInGame.InitAsync();

            var processor = new UnitProcessor();

            RunWave();
            RunUnits();
        }

        private void RunWave()
        {
            if (!db.stages.TryFind(userRepo.stage.Value, out var entity))
                return;

            var waves = db.waves.Map(entity.waveGroupId);
            _waveRunner.Init(waves, db.monsters, gameRepo.castle.Value, unitFactory);
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
                var unit = character.behaviour;
                unit.canAttack = true;
                unit.canMove = true;
            }
        }

        private void Exit(ExitRequest exitRequest)
        {
            var code = exitRequest.code;
            int stage = 0;
            UILobby.Step step;
            if (code != ExitCode.Exit)
            {
                step = UILobby.Step.InGame;
                int currentStage = userRepo.stage.Value;
                if (code == ExitCode.Retry)
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
                step = UILobby.Step.Lobby;
            }

            Storage.StartUpData = new LobbyStartUp
            {
                step = step,
                stage = stage
            };
            
            LoadSceneAfterDispose("SampleScene");
        }

        public override void Dispose()
        {
            base.Dispose();
            
            _uiInGame.Dispose();
        }
    }
}