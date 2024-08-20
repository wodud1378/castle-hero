using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using RGLabs.Data;
using RGLabs.InGame.Behaviours;
using RGLabs.Lobby.UI.Adapter;
using RGLabs.Network.Shared;
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

        [SerializeField] private UIItemList _rewardList;
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
            var transitions = GetTransition(data);
            var items = GetItems(result.data);

            UpdateUI(result);

            if (result.isCleared)
            {
                Context.sounds.PlaySfx(Storage.soundPath.gameClear);
                
                await UniTask.WhenAll(
                    _growthList.Init(transitions),
                    _rewardList.Init(items));

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

        private List<UnitTransition> GetTransition(GameCleared data)
        {
            List<UnitTransition> transitions = null;
            if (data is StageCleared stageResult)
            {
                transitions = stageResult.transitions;
            }

            if (transitions == null)
            {
                transitions = Storage.inGameRepository.characters
                    .Select(unit => UnitTransition.Create(unit.Info))
                    .ToList();
            }

            return transitions;
        }

        private List<IItem> GetItems(GameCleared data)
        {
            var items = new List<IItem>();
            if (data != null)
            {
                if (data.currency != null && !data.currency.IsEmpty())
                    items.AddRange(data.currency.ToItems());

                if (data.items is { Count: > 0 })
                    items.AddRange(data.items);
            }

            return items;
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

            if (!isCleared)
            {
                _retryButton.gameObject.SetActive(false);
                _nextButton.gameObject.SetActive(false);
                return;
            }

            // TODO : 행동력 체크.

            if (Storage.db.TryLoadNextGameEntity(result.type, result.data.id, out var entity))
            {
                _retryButton.gameObject.SetActive(true);
                _nextButton.gameObject.SetActive(true);
            }
        }

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