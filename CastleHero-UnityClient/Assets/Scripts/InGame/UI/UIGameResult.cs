using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.InGame.Behaviours;
using RGLabs.Network.Shared;
using RGLabs.Prepare.UI;
using RGLabs.Utility;
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

        [SerializeField] private Button _levelUpLink;
        [SerializeField] private Button _equipmentLink;
        [SerializeField] private Button _rateUpLink;

        [SerializeField] private UIRewardList _rewardList;
        [SerializeField] private UIUnitGrowthList _growthList;

        [SerializeField] private GameObject[] _clearObjects;
        [SerializeField] private GameObject[] _failedObjects;

        private void Awake()
        {
            this.SubscribeButton(_exitButton,()=> Exit());
            this.SubscribeButton(_retryButton, ()=>Retry());
            this.SubscribeButton(_nextButton, ()=>Next());

            this.SubscribeButton(_levelUpLink, () => Exit(Entrance.Link.LevelUp));
            this.SubscribeButton(_equipmentLink, () => Exit(Entrance.Link.Equipment));
            this.SubscribeButton(_rateUpLink, () => Exit(Entrance.Link.RateUp));
        }

        public async UniTaskVoid Open(GameResult result)
        {
            var data = result.data;

            UpdateUI(result);

            if (result.isCleared)
            {
                Context.sounds.PlaySfx(Storage.soundPath.gameClear);
                
                await UniTask.WhenAll(
                    _growthList.Init(data.transitions),
                    _rewardList.Init(data.GetRewardsForDisplay()));

                Activate();

                await UniTask.Delay(TimeSpan.FromSeconds(1f));

                _growthList.PlayDirection();
            }
            else
            {
                Context.sounds.PlaySfx(Storage.soundPath.gameFailed);
                
                Activate();
            }

            Context.Back.Add(this);
        }

        private void Activate()
        {
            gameObject.SetActive(true);
            _animtor.SetTrigger(EntranceHash);
        }

        private async void CloseWith(Action onClose)
        {
            _animtor.SetTrigger(ExitHash);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            gameObject.SetActive(false);
            onClose.Invoke();
        }

        private void UpdateUI(GameResult result)
        {
            bool isCleared = result.isCleared;
            foreach (var obj in _clearObjects)
                obj.SetActive(isCleared);

            foreach (var obj in _failedObjects)
                obj.SetActive(!isCleared);
            
            if(!isCleared)
                UpdateButtonsOnFailed();
            
            UpdateBottomButtons(result);
        }

        private void UpdateButtonsOnFailed()
        {
            var repository = Storage.userRepository;
            var enableLvLink = repository.UnitForLevelUp() != null;
            var enableRateLink = repository.UnitForUpgrade() != null;
            var enableEquipmentLink = repository.UnitForUpgradeEquipments(out _) != null;
            
            SetButtonActive(_levelUpLink, enableLvLink);
            SetButtonActive(_rateUpLink, enableRateLink);
            SetButtonActive(_equipmentLink, enableEquipmentLink);
        }

        private void UpdateBottomButtons(GameResult result)
        {
            int ap = Storage.userRepository.stamina.point.Value;
            bool activeRetry = HasApForRetry(ap, result.type, result.id);
            bool activeNext = HasApFoNext(ap, result.type, result.id) && result.isCleared;
            
            SetButtonActive(_retryButton, activeRetry);
            SetButtonActive(_nextButton, activeNext);
        }

        private void SetButtonActive(Button button, bool isActive)
        {
            button.interactable = isActive;

            if (button.TryGetComponent(out UIGrayScale grayScale))
                grayScale.enabled.Value = !isActive;
        }

        private bool HasApForRetry(int ap, GameType type, int id)
            => ap >= (Storage.db.TryLoadGameEntity(type, id, out var entity) ? entity.Ap : int.MaxValue);

        private bool HasApFoNext(int ap, GameType type, int id)
            => ap >= (Storage.db.TryLoadGameEntity(type, id, out var entity) ? entity.Ap : int.MaxValue);
        
        private void Exit(Entrance.Link link = Entrance.Link.None) => CloseWith(() =>
        {
            new ExitGame
            {
                code = ExitCode.Exit,
                link = link
            }.Publish();
        });

        private void Retry(Entrance.Link link = Entrance.Link.None) => CloseWith(() =>
        {
            new ExitGame
            {
                code = ExitCode.Retry,
                link = link
            }.Publish();
        });

        private void Next(Entrance.Link link = Entrance.Link.None) => CloseWith(() =>
        {
            new ExitGame
            {
                code = ExitCode.Next,
                link = link
            }.Publish();
        });

        public bool OnProcessBack()
        {
            Exit();
            return true;
        }
    }
}