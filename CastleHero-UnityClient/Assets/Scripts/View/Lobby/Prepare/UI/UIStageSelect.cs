using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Data.DB;
using CastleHero.Network.DB;
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

        private readonly List<IDisposable> _subscriptions = new();

        public void Init()
        {
            _repository = ServiceLocator.Get<IUserRepository>();
            _db = ServiceLocator.Get<IDBProvider>();

            this.SubscribeButton(prev, OnPrevStage);
            this.SubscribeButton(next, OnNextStage);

            _subscriptions.Add(_repository.Entrance
                .Subscribe(OnEntranceChanged));

            _subscriptions.Add(_stageData
                .Subscribe(x =>
                {
                    title.text = $"STAGE {x.Id}";
                    rewardList
                        .Init(x)
                        .Forget();

                    UpdateButtonsActive(x);
                }));

            var exist = _repository.Entrance.Value;
            if (exist.type == GameType.Dungeon && ServiceLocator.Get<EntranceHolder>().Current.state == State.Lobby)
            {
                _repository.Entrance.Value = new GameEntrance
                {
                    type = GameType.Stage,
                    id = _repository.StageFocus
                };
            }
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

            if (!stages.TryFindIndex(ServiceLocator.Get<IUserRepository>().GameRecord.LastClearedStage.Value, out int userIndex))
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
            var stages = _db.Stages;
            if (!stages.TryFindIndex(_stageData.Value.Id, out int index))
                return;

            index += adjust;
            if (!stages.IsValidIndex(index))
                return;

            if (!stages.TryIndexOf(index, out var entity))
                return;

            _repository.Entrance.Value = new GameEntrance
            {
                type = GameType.Stage,
                id = entity.Id
            };
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
