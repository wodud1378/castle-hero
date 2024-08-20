using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Lobby.Behaviours;
using RGLabs.Network.DB;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using UniRx;

namespace RGLabs.InGame
{
    public enum GameEvent
    {
        WaveDone,
        TimeOver,
        CastleDestroyed,
        DeadAllCharacters,
    }

    public struct GameFinished
    {
        public bool isCleared;
        public GameEvent cause;
        public GameType type;
        public int id;
    }
    
    public class GameHandler : IDisposable
    {
        public event Action<GameFinished> OnFinished;
        
        private readonly UserRepository _userRepo = Storage.userRepository;
        private readonly InGameRepository _gameRepo = Storage.inGameRepository;
        private readonly DBCollections _db = Storage.db;

        private readonly IGameEntity _entity;
        
        private readonly GameTimer _timer;
        private readonly UnitProcessor _unitProcessor;
        
        private readonly WaveRunner _wave;

        private readonly List<IDisposable> _subscriptions = new();
        private readonly List<GameEvent> _clearCondition = new();
        private readonly List<GameEvent> _failedCondition = new();
        
        public GameHandler(StartGame startGame, WaveRunner wave)
        {
            _entity = startGame.entity;
            _gameRepo.gameEntity = _entity;
            _gameRepo.leftTime.Value = _entity.TimeLimit;
            _wave = wave;
            
            _unitProcessor = new();
            _timer = new(_gameRepo.leftTime);

            if (_entity is DungeonEntity dungeonEntity)
            {
                switch (dungeonEntity.type)
                {
                    case DungeonType.Assault:
                    case DungeonType.Escort:
                        _clearCondition.Add(GameEvent.WaveDone);
                        _clearCondition.Add(GameEvent.TimeOver);
                        _failedCondition.Add(GameEvent.CastleDestroyed);
                        break;
                    case DungeonType.Raid:
                        _clearCondition.Add(GameEvent.WaveDone);
                        _failedCondition.Add(GameEvent.TimeOver);
                        _failedCondition.Add(GameEvent.DeadAllCharacters);
                        break;
                    case DungeonType.Invasion:
                        _clearCondition.Add(GameEvent.WaveDone);
                        _failedCondition.Add(GameEvent.TimeOver);
                        _failedCondition.Add(GameEvent.CastleDestroyed);
                        break;
                }
            }
            else
            {
                _clearCondition.Add(GameEvent.WaveDone);
                _failedCondition.Add(GameEvent.TimeOver);
                _failedCondition.Add(GameEvent.CastleDestroyed);
            }
        }

        public void OnStart()
        {
            Context.sounds.PlayBgm(_entity.Bgm);
            
            SetConditions();
            
            _timer.Run();
            _wave.Init(_entity.WaveId);
            _wave.isRunning = true;
            
            RunUnits();
        }

        public void OnExit(ExitGame exit)
        {
            _wave.isRunning = false;

            var data = NextEntrance(exit.code);
            Storage.entranceData = new Entrance
            {
                state = data.Item1,
                gameEntrance = data.Item2,
                link = exit.link,
            };
        }

        private void SetConditions()
        {
            if (_failedCondition.Contains(GameEvent.CastleDestroyed))
            {
                var castleSubscription = _gameRepo.castle.Value.state
                    .Where(x => x == UnitCore.States.Dead)
                    .Subscribe(_ => OccurGameEvent(GameEvent.CastleDestroyed));
            
                _subscriptions.Add(castleSubscription);
            }

            if (_failedCondition.Contains(GameEvent.DeadAllCharacters))
            {
                var deadSubscription = _gameRepo.deadCharacters
                    .ChangeAsObservable()
                    .Subscribe(x =>
                    {
                        int dead = x.Count(unit => unit.Type == UnitBehaviour.BehaviourType.Unit);
                        int all = _gameRepo.characters.Count(unit => unit.Type == UnitBehaviour.BehaviourType.Unit);
                        if (dead >= all)
                            OccurGameEvent(GameEvent.DeadAllCharacters);
                    });
                
                _subscriptions.Add(deadSubscription);
            }
            
            var waveSubscription = _wave.completed
                .Where(x => x)
                .Subscribe(_ => OccurGameEvent(GameEvent.WaveDone));
            
            _subscriptions.Add(waveSubscription);
            
            _timer.OnTimeOver += () => OccurGameEvent(GameEvent.TimeOver);
        }

        private void RunUnits()
        {
            bool enableRecover = !_failedCondition.Contains(GameEvent.DeadAllCharacters);
            foreach (var character in _gameRepo.characters)
            {
                var core = character.Core;
                core.onRest.Value = false;
                core.enableRecover = enableRecover;
            }
        }

        private void OccurGameEvent(GameEvent ev)
        {
            bool isEnd = false;
            bool isCleared = false;
            if (_clearCondition.Contains(ev))
            {
                isCleared = true;
                isEnd = true;
            }
            else if (_failedCondition.Contains(ev))
            {
                isEnd = true;
            }

            if (isEnd)
            {
                Context.sounds.StopBgm();
                
                OnFinished?.Invoke(new GameFinished
                {
                    isCleared = isCleared,
                    cause = ev,
                    type = _entity.Type,
                    id = _entity.Id,
                });
                
                OnFinished = null;
            }
        }

        private (State, GameEntrance) NextEntrance(ExitCode exitCode)
        {
            State state;
            GameEntrance entrance;
            if (exitCode != ExitCode.Exit)
            {
                state = State.InGame;
                var exist = _gameRepo.gameEntity;
                var entity = exitCode == ExitCode.Retry
                    ? exist
                    : Storage.db.TryLoadNextGameEntity(exist.Type, exist.Id, out var e)
                        ? e
                        : default;

                if (entity is not { IsValid: true })
                {
                    state = State.Lobby;
                    entrance = default;
                }
                else
                {
                    entrance = new GameEntrance
                    {
                        type = entity.Type,
                        id = entity.Id
                    };
                }
            }
            else
            {
                state = State.Lobby;
                entrance = default;
            }

            return (state, entrance);
        }
        
        public CastleSkillParameter[] GetCastleSkills()
        {
            int lv = _userRepo.gameRecord.castleLv.Value;
            if (!_db.castles.TryFind(lv, out var entity))
                return null;

            return entity.SkillParameters();
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _unitProcessor?.Dispose();
            
            _subscriptions.ForEach(x => x.Dispose());
        }
    }
}