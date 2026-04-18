using CastleHero.Common;
using CastleHero.Common.Flow;
using CastleHero.View.Common;
using CastleHero.View.Common.UI;
using CastleHero.View.Sound;
using CastleHero.Utility;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Bootstrapper
{
    public class ContextView : MonoBehaviour
    {
        [SerializeField] private int frameRate;
        [SerializeField] private GameConstants gameConstants;
        [SerializeField] private UILock uiLockField;
        [SerializeField] private UIToolTip toolTipField;
        [SerializeField] private StartButton startButtonField;
        [SerializeField] private PopupManager popupManager;
        [SerializeField] private SoundManager soundManager;

        public GameConstants GameConstants => gameConstants;
        public UILock UILock => uiLockField;
        public UIToolTip ToolTip => toolTipField;
        public StartButton StartButton => startButtonField;
        public PopupManager PopupManager => popupManager;
        public SoundManager SoundManager => soundManager;

        private Context _context;

        private async UniTaskVoid Start()
        {
            Application.targetFrameRate = frameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _context = await Bootstrapper.LoadAsync(this);

            startButtonField.enabled = _context.Transition.CurrentState == State.Lobby;
            InitSubscriptions();
        }

        private void InitSubscriptions()
        {
            this
                .UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Subscribe(_ => _context.ProcessBack())
                .AddTo(this);

            this.SubscribeButton(startButtonField, () => _context.LobbyTransition.CurrentState = LobbyState.Prepare);

            _context.Transition
                .StateObserver
                .Subscribe(x => { startButtonField.enabled = x == State.Lobby; })
                .AddTo(this);
        }
    }
}
