using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.Stage.UI
{
    public class UIStageSelect : MonoBehaviour, IDisposable
    {
        [SerializeField] private Button _prev;
        [SerializeField] private Button _next;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private RectTransform _rewardParent;
        [SerializeField] private AssetReference _rewardPrefab;

        private readonly List<UIItemSlot> _uiSlots = new();
        private readonly ReactiveProperty<StageEntity> _stageData = new(default);

        private UserRepository _repository;
        private DBCollections _db;

        private readonly List<IDisposable> _subscriptions = new();

        public void Init()
        {
            _repository = Storage.userRepository;
            _db = Storage.DB;
            
            this.SubscribeButton(_prev, OnPrevStage);
            this.SubscribeButton(_next, OnNextStage);
            
            _subscriptions.Add(_repository.stage.Subscribe(OnStageSelected));
            _subscriptions.Add(_stageData
                .Subscribe(x =>
                {
                    _title.text = $"STAGE {x.Id}";
                    SetRewards(x);
                    UpdateButtonsActive(x);
                }));

            OnStageSelected(_repository.stage.Value);
        }

        public void SetMoveStageEnable(bool enabled)
        {
            _prev.gameObject.SetActive(enabled);
            _next.gameObject.SetActive(enabled);
        }

        private async void SetRewards(StageEntity stageData)
        {
            foreach (var slot in _uiSlots)
            {
                Addressables.ReleaseInstance(slot.gameObject);
            }
            _uiSlots.Clear();
            
            var rewards = _db.rewards.Map(stageData.rewards);
            foreach (var reward in rewards)
            {
                if (!_db.items.TryFind(reward.itemId, out var entity))
                    continue;

                var obj = await Addressables.InstantiateAsync(_rewardPrefab, _rewardParent);
                if (!obj.TryGetComponent(out UIItemSlot slot))
                    continue;

                _uiSlots.Add(slot);

                await slot.InitAsync(entity.icon);
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
            if (!stages.TryFindIndex(stageData.Id, out int index))
                index = 0;

            _prev.gameObject.SetActive(index > 0);
            _next.gameObject.SetActive(index < stages.Length - 1);
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

            _repository.stage.Value = entity.Id;
        }

        public void Dispose()
        {
            foreach (var slot in _uiSlots)
            {
                slot.Dispose();
                Addressables.ReleaseInstance(slot.gameObject);
            }

            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
        }
    }
}