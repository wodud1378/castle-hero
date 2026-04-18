using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Flow;
using CastleHero.Common.Sound;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.InGame.System;
using CastleHero.Data.DB;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.Utility;
using UniRx;
using CastleHero.Common.Pattern;
// IInGameSession은 동일 네임스페이스(CastleHero.GamePlay.InGame)에 위치.

namespace CastleHero.GamePlay.InGame
{
    public class GameHandler : IDisposable
    {
        private readonly UniTaskCompletionSource<GameFinished> _finished = new();
        public UniTask<GameFinished> Finished => _finished.Task;

        private readonly IUserRepository _userRepo;
        private readonly IInGameSession _gameSession;
        private readonly IDBProvider _db;
        private readonly ISoundManager _sounds;
        private readonly EntranceHolder _entranceHolder;

        private readonly IGameEntity _entity;

        private readonly GameTimer _timer;
        private readonly UnitProcessor _unitProcessor;

        private readonly IWaveController _wave;

        private readonly List<IDisposable> _subscriptions = new();
        private readonly List<GameEvent> _clearCondition = new();
        private readonly List<GameEvent> _failedCondition = new();

        public GameHandler(StartGame startGame, IWaveController wave,
            IUserRepository userRepo, IInGameSession gameSession, IDBProvider db, ISoundManager sounds,
            EffectBuilder effectBuilder, GameConstants constants)
        {
            _userRepo = userRepo;
            _gameSession = gameSession;
            _db = db;
            _sounds = sounds;
            _entranceHolder = ServiceLocator.Instance.Get<EntranceHolder>();

            _entity = startGame.entity;
            _gameSession.GameEntity = _entity;
            _gameSession.LeftTime.Value = _entity.TimeLimit;
            _wave = wave;

            _unitProcessor = new(_gameSession, effectBuilder, sounds, constants);
            _timer = new(_gameSession.LeftTime);

            if (_entity is DungeonEntity dungeonEntity)
            {
                switch (dungeonEntity.category)
                {
                    case DungeonCategory.Assault:
                    case DungeonCategory.Escort:
                        _clearCondition.Add(GameEvent.WaveDone);
                        _clearCondition.Add(GameEvent.TimeOver);
                        _failedCondition.Add(GameEvent.CastleDestroyed);
                        break;
                    case DungeonCategory.Raid:
                        _clearCondition.Add(GameEvent.WaveDone);
                        _failedCondition.Add(GameEvent.TimeOver);
                        _failedCondition.Add(GameEvent.DeadAllCharacters);
                        break;
                    case DungeonCategory.Invasion:
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
            _sounds.PlayBgm(_entity.Bgm);
            
            SetConditions();
            
            _timer.Run();
            _wave.Init(_entity.WaveId, _db, _gameSession);
            _wave.IsRunning = true;
            
            RunUnits();
        }

        public void OnExit(ExitGame exit)
        {
            _wave.IsRunning = false;

            var data = NextEntrance(exit.code);
            _entranceHolder.Current = new Entrance
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
                var castleSubscription = ((UnitActor)_gameSession.Castle.Value).State
                    .Where(x => x == UnitState.States.Dead)
                    .Subscribe(_ => OccurGameEvent(GameEvent.CastleDestroyed));

                _subscriptions.Add(castleSubscription);
            }

            if (_failedCondition.Contains(GameEvent.DeadAllCharacters))
            {
                var units = _gameSession.Characters
                    .OfType<UnitActor>()
                    .Where(x => x.Type == UnitActor.ActorType.Unit)
                    .ToList();

                foreach (var unit in units)
                {
                    var sub = unit.OnDead.Take(1).Subscribe(_ =>
                    {
                        if (units.TrueForAll(x => x.State.Value == UnitState.States.Dead))
                            OccurGameEvent(GameEvent.DeadAllCharacters);
                    });

                    _subscriptions.Add(sub);
                }
            }
            
            var waveSubscription = _wave.Completed
                .Where(x => x)
                .Subscribe(_ => OccurGameEvent(GameEvent.WaveDone));
            
            _subscriptions.Add(waveSubscription);
            
            _timer.OnTimeOver += () => OccurGameEvent(GameEvent.TimeOver);
        }

        private void RunUnits()
        {
            bool enableRecover = !_failedCondition.Contains(GameEvent.DeadAllCharacters);
            foreach (var character in _gameSession.Characters.OfType<UnitActor>())
            {
                character.UnitState.OnRest.Value = false;
                character.UnitState.EnableRecover = enableRecover;
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
                _sounds.StopBgm();

                _finished.TrySetResult(new GameFinished
                {
                    IsCleared = isCleared,
                    Cause = ev,
                    Type = _entity.Type,
                    Id = _entity.Id,
                });
            }
        }

        private (State, GameEntrance) NextEntrance(ExitCode exitCode)
        {
            State state;
            GameEntrance entrance;
            if (exitCode != ExitCode.Exit)
            {
                state = State.InGame;
                var exist = _gameSession.GameEntity;
                var entity = exitCode == ExitCode.Retry
                    ? exist
                    : _db.TryLoadNextGameEntity(exist.Type, exist.Id, out var e)
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
            int lv = _userRepo.GameRecord.CastleLv.Value;
            if (!_db.Castles.TryFind(lv, out var entity))
                return global::System.Array.Empty<CastleSkillParameter>();

            return entity.SkillParameters() ?? global::System.Array.Empty<CastleSkillParameter>();
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _unitProcessor?.Dispose();
            
            _subscriptions.ForEach(x => x.Dispose());
        }
    }
}