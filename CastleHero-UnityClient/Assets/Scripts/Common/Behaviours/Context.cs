using System;
using System.Collections.Generic;
using System.Threading;
using RGLabs.Common.Flow;
using RGLabs.Common.Pattern;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Unit.Factory;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Common.Behaviours
{
    public class Context : MonoBehaviour
    {
        public static Queue<Action> OnLoadCompleteQueue = new();
        public static readonly BackButton Back = new();
        public static readonly Transition Transition = new();

        public static PoolContainer poolContainer;
        public static UnitFactory unitFactory;
        public static CastleFactory castleFactory;
        
        public static StartButton startButton;
        public static PopupManager popupManager;
        public static UILock uiLock;
        public static UIToolTip toolTip;
        
        [SerializeField] private int _frameRate;
        [SerializeField] private UILock _uiLock;
        [SerializeField] private UIToolTip _toolTip;
        [SerializeField] private StartButton _startButton;
        [SerializeField] private PopupManager _popupManager;

        private void Load()
        {
            Time.timeScale = 1f;
         
            try
            {
                poolContainer = new();
                unitFactory = new UnitFactory();
                castleFactory = new CastleFactory();
            
                uiLock = _uiLock;
                toolTip = _toolTip;
                startButton = _startButton;
                startButton.StageSelect.Init();
            
                popupManager = _popupManager;
            
                InitSubscriptions();

                while (OnLoadCompleteQueue.Count > 0)
                {
                    OnLoadCompleteQueue.Dequeue()?.Invoke();
                }
            
                SetEntranceTransition();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        
        private void Start()
        {
            Application.targetFrameRate = _frameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            Load();
        }

        private void SetEntranceTransition()
        {
            if (Storage.entranceData.state == State.InGame)
            {
                Storage.userRepository.entrance.Value = Storage.entranceData.gameEntrance;
                return;
            }

            startButton.enabled = true;
            Transition.CurrentState = Storage.entranceData.state;
        }

        private void InitSubscriptions()
        {
            this
                .UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(KeyCode.Escape))
                .Subscribe(_=> ProcessBack())
                .AddTo(this);

            this.SubscribeButton(startButton, () => Transition.CurrentState = State.Prepare);
            
            Transition
                .StateObserver
                .Subscribe(x => { startButton.enabled = x == State.Lobby; })
                .AddTo(this);
        }

        private void ProcessBack()
        {
            if (Back.ProcessBack(out var failedCause))
                return;

            if (failedCause == BackButton.FailedCause.NoListeners)
            {
                // TODO 게임 종료 팝업
            }
        }
    }
}