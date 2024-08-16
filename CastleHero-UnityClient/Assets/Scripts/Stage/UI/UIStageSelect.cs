using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.Network.DB;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Stage.UI
{
    public class UIStageSelect : MonoBehaviour, IDisposable
    {
        [SerializeField] private Button _prev;
        [SerializeField] private Button _next;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private UIStageRewardList _rewardList;
        
        private readonly ReactiveProperty<StageEntity> _stageData = new(default);

        private UserRepository _repository;
        private DBCollections _db;

        private readonly List<IDisposable> _subscriptions = new();

        public void Init()
        {
            _repository = Storage.userRepository;
            _db = Storage.db;

            this.SubscribeButton(_prev, OnPrevStage);
            this.SubscribeButton(_next, OnNextStage);

            _subscriptions.Add(_repository.stageFocus.Subscribe(OnStageSelected));
            _subscriptions.Add(_stageData
                .Subscribe(x =>
                {
                    _title.text = $"STAGE {x.Id}";
                    _rewardList
                        .Init(x)
                        .Forget();
                    
                    UpdateButtonsActive(x);
                }));

            OnStageSelected(_repository.stageFocus.Value);
        }

        public void SetMoveStageEnable(bool enabled)
        {
            if (enabled)
            {
                UpdateButtonsActive(_stageData.Value);
            }
            else
            {
                _prev.gameObject.SetActive(false);
                _next.gameObject.SetActive(false);
            }
        }

        private void OnStageSelected(int stage)
        {
            if (!_db.stages.TryFind(stage, out var entity))
                return;

            _stageData.Value = entity;
        }

        private void UpdateButtonsActive(StageEntity stageData)
        {
            var stages = _db.stages;
            if (!stages.TryFindIndex(stageData.Id, out int dataIndex))
                dataIndex = 0;

            if (!stages.TryFindIndex(Storage.userRepository.gameRecord.lastClearedStage.Value, out int userIndex))
                userIndex = 0;

            _prev.gameObject.SetActive(dataIndex > 0);
            _next.gameObject.SetActive(
                userIndex >= dataIndex &&
                dataIndex < stages.Length - 1);
        }

        #region UI Events.

        private void OnPrevStage() => AdjustIndex(-1);

        private void OnNextStage() => AdjustIndex(1);

        #endregion

        private void AdjustIndex(int adjust)
        {
            var stages = _db.stages;
            if (!stages.TryFindIndex(_stageData.Value.Id, out int index))
                return;

            index += adjust;
            if (!stages.IsValidIndex(index))
                return;

            if (!stages.TryIndexOf(index, out var entity))
                return;

            _repository.stageFocus.Value = entity.Id;
        }

        public void Dispose()
        {
            _rewardList.Dispose();

            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
        }
    }
}