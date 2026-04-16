using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Flow;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.View.Common;
using CastleHero.View.Common.UI;
using CastleHero.View.Sound;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Factory;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.GamePlay.Unit.Factory;
using CastleHero.View.Unit;
using CastleHero.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

using CastleHero.Network;
namespace CastleHero.View.Bootstrapper
{
    /// <summary>
    /// 씬 루트 부트스트래퍼. 초기화 완료 후 모든 뷰 공용 서비스를 ServiceLocator 에 등록한다.
    /// 이전에는 public static 필드를 직접 보유했으나, 현재는 ServiceLocator 기반으로 재구성됨.
    /// </summary>
    public class Context : MonoBehaviour
    {
        /// <summary>
        /// 씬 로드 이후 실행돼야 할 작업 큐. SceneBehaviour 가 사용.
        /// 정적으로 유지하는 이유: 서브 씬 비헤이비어들이 Awake 순서에 상관없이 큐에 enqueue 해야 함.
        /// 라이프사이클: Context.LoadAsync 에서 1회 Drain 후 Clear 된다. 씬 재진입 시에도 항상 빈 상태로 시작.
        /// </summary>
        public static readonly Queue<Func<UniTask>> OnLoadCompleteQueue = new();

#if UNITY_EDITOR
        [SerializeField] private NetworkConfig networkConfig;
#endif

        [SerializeField] private int frameRate;
        [SerializeField] private GameConstants gameConstants;
        [SerializeField] private UILock uiLockField;
        [SerializeField] private UIToolTip toolTipField;
        [SerializeField] private StartButton startButtonField;
        [SerializeField] private PopupManager popupManager;
        [SerializeField] private SoundManager soundManager;

        private BackButton _back;
        private StateManager<State> _transition;
        private StateManager<LobbyState> _lobbyTransition;
        private StartButton _startButton;

        private async UniTask LoadAsync()
        {
            Time.timeScale = 1f;

            try
            {
                // --- 핵심 상태/흐름 객체 생성 ---
                _back = new BackButton();
                _transition = new StateManager<State>(State.None);
                _lobbyTransition = new StateManager<LobbyState>(LobbyState.None);

                ServiceLocator.Register(_back);
                ServiceLocator.Register(_transition);
                ServiceLocator.Register(_lobbyTransition);

                // --- GamePlay 공용 서비스 (부트스트랩 지점 — 여기서만 ServiceLocator.Register/Get 허용) ---
                if (gameConstants == null) gameConstants = ScriptableObject.CreateInstance<GameConstants>();
                ServiceLocator.Register(gameConstants);

                var poolContainer = new PoolContainer();
                ServiceLocator.Register(poolContainer);
                ServiceLocator.Register<ISoundManager>(soundManager);
                ServiceLocator.Register(new EffectBuilder(poolContainer));
                ServiceLocator.Register<IUnitRendererFactory>(new UnitRendererFactory());

                // Factory 생성에는 IDBProvider (BackendBootService 가 이미 등록), IUserRepository 필요.
                var db = ServiceLocator.Get<IDBProvider>();
                var userRepo = ServiceLocator.Get<IUserRepository>();
                ServiceLocator.Register<IUnitFactory>(new UnitFactory(poolContainer, db, userRepo));
                ServiceLocator.Register<ICastleFactory>(new CastleFactory(poolContainer, db));

#if UNITY_EDITOR
                NetworkConfig.Current = networkConfig;
#endif

                // --- View 공용 서비스 ---
                ServiceLocator.Register(uiLockField);
                ServiceLocator.Register(toolTipField);
                _startButton = startButtonField;
                ServiceLocator.Register(_startButton);
                _startButton.StageSelect.Init();

                ServiceLocator.Register<IPopupManager>(popupManager);

                InitSubscriptions();

                // Drain OnLoadCompleteQueue. 순차 실행 후 Clear (예외 중단 시에도 잔여물 없도록 finally).
                try
                {
                    while (OnLoadCompleteQueue.Count > 0)
                    {
                        var task = OnLoadCompleteQueue.Dequeue();
                        if (task != null) await task();
                    }
                }
                finally
                {
                    OnLoadCompleteQueue.Clear();
                }

                SetEntranceTransition();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            finally
            {
                // Loading 씬이 이 시그널을 기다린다. 예외 발생 시에도 반드시 해제.
                Loading.SceneReady?.TrySetResult();
            }
        }

        private async UniTaskVoid Start()
        {
            Application.targetFrameRate = frameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            await LoadAsync();
        }

        private void SetEntranceTransition()
        {
            var entranceHolder = ServiceLocator.Get<EntranceHolder>();
            var entrance = entranceHolder.Current;
            if (entrance.state == State.InGame)
            {
                ServiceLocator.Get<IUserRepository>().Entrance.Value = entrance.gameEntrance;
                return;
            }

            _startButton.enabled = true;
            _transition.CurrentState = entrance.state;
            _lobbyTransition.CurrentState = LobbyState.Main;

            ServiceLocator.Get<ISoundManager>().PlayBgm(ServiceLocator.Get<SoundPath>().lobbyBgm);
        }

        private void InitSubscriptions()
        {
            this
                .UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Subscribe(_ => ProcessBack())
                .AddTo(this);

            this.SubscribeButton(_startButton, () => _lobbyTransition.CurrentState = LobbyState.Prepare);

            _transition
                .StateObserver
                .Subscribe(x => { _startButton.enabled = x == State.Lobby; })
                .AddTo(this);
        }

        private void ProcessBack()
        {
            if (_back.ProcessBack(out var failedCause))
                return;

            if (failedCause == BackButton.FailedCause.NoListeners)
            {
                // TODO 게임 종료 팝업
            }
        }
    }
}
