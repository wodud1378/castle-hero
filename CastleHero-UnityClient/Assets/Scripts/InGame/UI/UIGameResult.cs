using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.InGame.Behaviours;
using RGLabs.Lobby.UI.Adapter;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIGameResult : MonoBehaviour, IBackButtonListener
    {
        private static readonly int EntranceHash = Animator.StringToHash("Entrance");
        private static readonly int ExitHash = Animator.StringToHash("Exit");

        [SerializeField] private Animator _animtor;

        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _nextButton;

        [SerializeField] private UIItemList _rewardList;
        [SerializeField] private UIUnitGrowthList _growthList;

        [SerializeField] private GameObject[] _clearObjects;
        [SerializeField] private GameObject[] _failedObjects;

        private void Awake()
        {
            this.SubscribeButton(_exitButton, Exit);
            this.SubscribeButton(_retryButton, Retry);
            this.SubscribeButton(_nextButton, Next);
        }

        public void Open(GameResult result)
        {
            UpdateUI(result.isCleared);

            if (result.isCleared)
            {
                var data = result.data;
                var initTask = _growthList.Init(data.transitions);
                var entranceTask = TaskHelper.OnAnimationEnd(_animtor, EntranceHash);
                var combined = UniTask.WhenAll(initTask, entranceTask);

                combined
                    .ContinueWith(_growthList.PlayDirection)
                    .Forget();
            }

            gameObject.SetActive(true);

            Context.Back.Add(this);
        }

        private async void CloseWith(Action onClose)
        {
            await TaskHelper.OnAnimationEnd(_animtor, ExitHash);

            gameObject.SetActive(false);
            onClose.Invoke();
        }

        private void UpdateUI(bool isCleared)
        {
            foreach (var obj in _clearObjects)
                obj.SetActive(isCleared);

            foreach (var obj in _failedObjects)
                obj.SetActive(!isCleared);

            var db = Storage.db.stages;
            int currentStage = Storage.userRepository.focusedStage.Value;
            int lastStageIndex = db.Length;
            if (db.TryFindIndex(currentStage, out int index))
                _nextButton.gameObject.SetActive(index < lastStageIndex);
            else
                _nextButton.gameObject.SetActive(false);
        }

        private void Exit() => CloseWith(() => ExitCode.Exit.Publish());

        private void Retry() => CloseWith(() => ExitCode.Retry.Publish());

        private void Next() => CloseWith(() => ExitCode.Next.Publish());

        public bool OnProcessBack()
        {
            Exit();
            return true;
        }
    }
}