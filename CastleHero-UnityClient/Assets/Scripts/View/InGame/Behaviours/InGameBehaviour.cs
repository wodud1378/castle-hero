using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.Common.Flow;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.InGame;
using CastleHero.GamePlay.InGame.Behaviours;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.Network.Shared;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;
using CastleHero.View.InGame.Actions;
using CastleHero.View.InGame.UI;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.InGame.Behaviours
{
    public class InGameScene : SceneBase
    {
        [FormerlySerializedAs("_uiInGame")]
        [SerializeField] private UIInGame uiInGame;
        [FormerlySerializedAs("_waveRunner")]
        [SerializeField] private WaveRunner waveRunner;

        // Awake-time 캐싱.
        private IDBProvider _db;
        private IUserRepository _userRepo;
        private ISoundManager _sounds;
        private EffectBuilder _effectBuilder;
        private GameConstants _constants;
        private ISettingRepository _settings;
        private IPopupManager _popups;
        private StartButton _startButton;
        private PoolContainer _poolContainer;
        private EntranceHolder _entranceHolder;

        private GameClearAction _gameClearAction;

        private GameHandler _handler;
        private InGameSession _session;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _db = sl.Get<IDBProvider>();
            _userRepo = sl.Get<IUserRepository>();
            _sounds = sl.Get<ISoundManager>();
            _effectBuilder = sl.Get<EffectBuilder>();
            _constants = sl.Get<GameConstants>();
            _settings = sl.Get<ISettingRepository>();
            _popups = sl.Get<IPopupManager>();
            _startButton = sl.Get<StartButton>();
            _poolContainer = sl.Get<PoolContainer>();
            _entranceHolder = sl.Get<EntranceHolder>();

            _gameClearAction = sl.Get<GameClearAction>();

            this.SubscribeMessage<ExitGame>(Exit);
            this.SubscribeMessage<StartGame>(Run);
        }

        protected override async UniTask OnLoaded()
        {
            var entrance = _entranceHolder.Current.gameEntrance;
            var planner = new StagePreloadPlanner(_db, _userRepo);
            var entries = planner.Plan(entrance.id);

            var preloader = new Preloader();
            await preloader.PreloadAll(entries, _poolContainer);
        }

        private void Run(StartGame startGame)
        {
            _session = new InGameSession(startGame.draft);

            Time.timeScale = _settings.speedUp.Value ? 2f : 1f;

            var cam = Camera.main;
            cam.DOOrthoSize(20f, 0.4f);

            uiInGame.gameObject.SetActive(true);
            uiInGame.Init();
            uiInGame.Open();

            _startButton.enabled = false;

            _handler = new GameHandler(startGame, waveRunner,
                _userRepo, _session, _db, _sounds, _effectBuilder, _constants);
            AwaitFinished().SafeForget();

            InitGlobalSkills();

            _handler.OnStart();
        }

        private async UniTask AwaitFinished()
        {
            var finished = await _handler.Finished;
            OnFinishedAsync(finished).SafeForget();
        }

        private async UniTaskVoid OnFinishedAsync(GameFinished finished)
        {
            finished.Publish();

            var isCleared = finished.IsCleared;
            var type = finished.Type;
            var id = finished.Id;
            GameCleared data = null;
            if (isCleared)
            {
                var result = await _gameClearAction.RequestClear(type, id);
                if (!result.IsSuccess)
                {
                    var param = new PopupCommon.ButtonParam
                    {
                        action = PopupCommon.ButtonAction.Confirm,
                        onClick = () =>
                        {
                            Exit(new ExitGame
                            {
                                code = ExitCode.Exit,
                                link = Entrance.Link.None
                            });
                        }
                    };

                    _popups.Open<PopupCommon>(result.error, param);
                    return;
                }

                data = result.data;
            }

            new GameResult
            {
                IsCleared = isCleared,
                Id = id,
                Type = type,
                Data = data
            }.Publish();

            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }

        private void InitGlobalSkills()
        {
            var ui = uiInGame.GlobalSkill;
            if (_session.Castle.Value != null)
            {
                ui.gameObject.SetActive(true);
                ui.Init(_handler.GetCastleSkills());
            }
            else
            {
                ui.gameObject.SetActive(false);
            }
        }

        private void Exit(ExitGame exit)
        {
            _handler.OnExit(exit);

            Loading.Tasks.Add(_gameClearAction.RefreshUserData());
            LoadSceneAfterDispose("Main");
        }

        public override void Dispose()
        {
            base.Dispose();

            uiInGame.Dispose();
            _handler?.Dispose();
            _session?.Dispose();

            // 스테이지에서 로드한 프리팹/Addressables 핸들을 모두 해제.
            // 다음 씬의 Context 가 Start 에서 새로운 PoolContainer 를 ServiceLocator 에 덮어쓴다.
            _poolContainer?.Dispose();
        }
    }
}
