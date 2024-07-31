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

        public async UniTaskVoid Open(GameResult result)
        {
            await UniTask.WhenAll(
                _growthList.Init(result.data.transitions),
                _rewardList.Init(result.data.items));

            gameObject.SetActive(true);

            UpdateUI(result.isCleared);

            if (result.isCleared)
            {
                _animtor.SetTrigger(EntranceHash);
                await UniTask.Delay(TimeSpan.FromSeconds(1f));

                _growthList.PlayDirection();
            }

            Context.Back.Add(this);
        }

        private async void CloseWith(Action onClose)
        {
            _animtor.SetTrigger(ExitHash);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));

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