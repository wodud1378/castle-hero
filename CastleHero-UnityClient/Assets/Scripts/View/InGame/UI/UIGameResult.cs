using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.GamePlay.InGame.Behaviours;
using CastleHero.View.InGame.Behaviours;
using CastleHero.Network.Shared;
using CastleHero.View.Lobby.Prepare.UI;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.GamePlay.InGame;

using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
namespace CastleHero.View.InGame.UI
{
    public class UIGameResult : MonoBehaviour, IBackButtonListener
    {
        private static readonly int EntranceHash = Animator.StringToHash("Entrance");
        private static readonly int ExitHash = Animator.StringToHash("Exit");

        [FormerlySerializedAs("_animtor")]
        [SerializeField] private Animator animtor;

        [FormerlySerializedAs("_exitButton")]
        [SerializeField] private Button exitButton;
        [FormerlySerializedAs("_retryButton")]
        [SerializeField] private Button retryButton;
        [FormerlySerializedAs("_nextButton")]
        [SerializeField] private Button nextButton;

        [FormerlySerializedAs("_levelUpLink")]
        [SerializeField] private Button levelUpLink;
        [FormerlySerializedAs("_equipmentLink")]
        [SerializeField] private Button equipmentLink;
        [FormerlySerializedAs("_rateUpLink")]
        [SerializeField] private Button rateUpLink;

        [FormerlySerializedAs("_rewardList")]
        [SerializeField] private UIRewardList rewardList;
        [FormerlySerializedAs("_growthList")]
        [SerializeField] private UIUnitGrowthList growthList;

        [FormerlySerializedAs("_clearObjects")]
        [SerializeField] private GameObject[] clearObjects;
        [FormerlySerializedAs("_failedObjects")]
        [SerializeField] private GameObject[] failedObjects;

        private void Awake()
        {
            this.SubscribeButton(exitButton,()=> Exit());
            this.SubscribeButton(retryButton, ()=>Retry());
            this.SubscribeButton(nextButton, ()=>Next());

            this.SubscribeButton(levelUpLink, () => Exit(Entrance.Link.LevelUp));
            this.SubscribeButton(equipmentLink, () => Exit(Entrance.Link.Equipment));
            this.SubscribeButton(rateUpLink, () => Exit(Entrance.Link.RateUp));
        }

        public async UniTaskVoid Open(GameResult result)
        {
            var data = result.Data;

            UpdateUI(result);

            if (result.IsCleared)
            {
                ServiceLocator.Get<ISoundManager>().PlaySfx(ServiceLocator.Get<SoundPath>().gameClear);

                await UniTask.WhenAll(
                    growthList.Init(data.transitions),
                    rewardList.Init(data.GetRewardsForDisplay()));

                Activate();

                await UniTask.Delay(TimeSpan.FromSeconds(1f));

                growthList.PlayDirection();
            }
            else
            {
                ServiceLocator.Get<ISoundManager>().PlaySfx(ServiceLocator.Get<SoundPath>().gameFailed);

                Activate();
            }

            ServiceLocator.Get<BackButton>().Add(this);
        }

        private void Activate()
        {
            gameObject.SetActive(true);
            animtor.SetTrigger(EntranceHash);
        }

        private async UniTask CloseWith(Action onClose)
        {
            animtor.SetTrigger(ExitHash);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            gameObject.SetActive(false);
            onClose.Invoke();
        }

        private void UpdateUI(GameResult result)
        {
            bool isCleared = result.IsCleared;
            foreach (var obj in clearObjects)
                obj.SetActive(isCleared);

            foreach (var obj in failedObjects)
                obj.SetActive(!isCleared);

            if(!isCleared)
                UpdateButtonsOnFailed();

            UpdateBottomButtons(result);
        }

        private void UpdateButtonsOnFailed()
        {
            var repository = ServiceLocator.Get<IUserRepository>();
            var enableLvLink = repository.UnitForLevelUp() != null;
            var enableRateLink = repository.UnitForUpgrade() != null;
            var enableEquipmentLink = repository.UnitForUpgradeEquipments(out _) != null;

            SetButtonActive(levelUpLink, enableLvLink);
            SetButtonActive(rateUpLink, enableRateLink);
            SetButtonActive(equipmentLink, enableEquipmentLink);
        }

        private void UpdateBottomButtons(GameResult result)
        {
            int ap = ServiceLocator.Get<IUserRepository>().Stamina.Point.Value;
            bool activeRetry = HasApForRetry(ap, result.Type, result.Id);
            bool activeNext = HasApFoNext(ap, result.Type, result.Id) && result.IsCleared;

            SetButtonActive(retryButton, activeRetry);
            SetButtonActive(nextButton, activeNext);
        }

        private void SetButtonActive(Button button, bool isActive)
        {
            button.interactable = isActive;

            if (button.TryGetComponent(out UIGrayScale grayScale))
                grayScale.enabled.Value = !isActive;
        }

        private bool HasApForRetry(int ap, GameType type, int id)
            => ap >= (ServiceLocator.Get<IDBProvider>().TryLoadGameEntity(type, id, out var entity) ? entity.Ap : int.MaxValue);

        private bool HasApFoNext(int ap, GameType type, int id)
            => ap >= (ServiceLocator.Get<IDBProvider>().TryLoadGameEntity(type, id, out var entity) ? entity.Ap : int.MaxValue);

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
