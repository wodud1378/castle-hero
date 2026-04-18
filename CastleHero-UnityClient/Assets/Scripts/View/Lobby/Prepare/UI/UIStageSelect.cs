using System;
using System.Collections.Generic;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Data.DB;
using CastleHero.Data.UseCases;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
namespace CastleHero.View.Lobby.Prepare.UI
{
    public class UIStageSelect : MonoBehaviour, IStageSelect
    {
        [FormerlySerializedAs("_prev")]
        [SerializeField] private Button prev;
        [FormerlySerializedAs("_next")]
        [SerializeField] private Button next;

        [FormerlySerializedAs("_title")]
        [SerializeField] private TMP_Text title;
        [FormerlySerializedAs("_rewardList")]
        [SerializeField] private UIRewardList rewardList;

        private readonly ReactiveProperty<StageEntity> _stageData = new(default);

        private IUserRepository _repository;
        private IDBProvider _db;
        private EntranceManager _entranceManager;

        private readonly List<IDisposable> _subscriptions = new();

        private void Awake()
        {
            var sl = ServiceLocator.Instance;
            _repository = sl.Get<IUserRepository>();
            _db = sl.Get<IDBProvider>();

            _entranceManager = sl.Get<EntranceManager>();
        }

        public void Init()
        {

            this.SubscribeButton(prev, OnPrevStage);
            this.SubscribeButton(next, OnNextStage);

            _subscriptions.Add(_repository.Entrance
                .Subscribe(OnEntranceChanged));

            _subscriptions.Add(_stageData
                .Subscribe(x =>
                {
                    title.text = $"STAGE {x.Id}";
                    rewardList.Init(x);

                    UpdateButtonsActive(x);
                }));

            _entranceManager.RevertToStageOnLobbyInit(
                ServiceLocator.Instance.Get<EntranceHolder>());
        }

        public void SetMoveStageEnable(bool enabled)
        {
            if (enabled)
            {
                UpdateButtonsActive(_stageData.Value);
            }
            else
            {
                prev.gameObject.SetActive(false);
                next.gameObject.SetActive(false);
            }
        }

        private void OnEntranceChanged(GameEntrance entrance)
        {
            if (entrance.type != GameType.Stage)
                return;

            if (!_db.Stages.TryFind(entrance.id, out var entity))
                return;

            _stageData.Value = entity;
        }

        private void UpdateButtonsActive(StageEntity stageData)
        {
            var stages = _db.Stages;
            if (!stages.TryFindIndex(stageData.Id, out int dataIndex))
                dataIndex = 0;

            if (!stages.TryFindIndex(_repository.GameRecord.LastClearedStage.Value, out int userIndex))
                userIndex = -1;

            prev.gameObject.SetActive(dataIndex > 0);
            next.gameObject.SetActive(
                userIndex >= dataIndex &&
                dataIndex < stages.Length - 1);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                AdjustIndex(-10);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                AdjustIndex(10);
            }
        }
#endif

        #region UI Events.

        private void OnPrevStage() => AdjustIndex(-1);

        private void OnNextStage() => AdjustIndex(1);

        #endregion

        private void AdjustIndex(int adjust)
        {
            _entranceManager.AdjustStageIndex(_stageData.Value.Id, adjust);
        }

        public void Dispose()
        {
            rewardList.Dispose();

            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
        }
    }
}
