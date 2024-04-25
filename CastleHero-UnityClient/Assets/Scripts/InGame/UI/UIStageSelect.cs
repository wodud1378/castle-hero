using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIStageSelect : MonoBehaviour, IDisposable
    {
        public struct Result
        {
            public int selectedStage;
        }
        
        [SerializeField] private UIStage _ui;
        [SerializeField] private Button _prev;
        [SerializeField] private Button _next;
        [SerializeField] private Button _complete;

        private readonly ReactiveProperty<int> _stageIndex = new(0);
        private InGameRepository _repository;
        private InGameDB _db;

        private IDisposable _subscription;

        private void Awake()
        {
            _prev
                .OnClickAsObservable()
                .Subscribe(_=> OnPrevStage())
                .AddTo(this);

            _next
                .OnClickAsObservable()
                .Subscribe(_ => OnNextStage())
                .AddTo(this);

            _complete
                .OnClickAsObservable()
                .Subscribe(_ => OnClickSelect())
                .AddTo(this);
        }

        public void Open(InGameRepository repository, InGameDB db)
        {
            Init(repository, db);
            
            gameObject.SetActive(true);
        }
        
        private void Init(InGameRepository repository, InGameDB db)
        {
            _repository = repository;
            _db = db;
            _ui.Init(this, db.rewards, db.items);

            _subscription = _repository.stage.Subscribe(OnStageSelected);

            int stage = _repository.stage.Value;
            
            OnStageSelected(stage);
            SetIndex(stage);
        }

        private void SetIndex(int stage)
        {
            if (!_db.stages.TryFindIndex(stage, out int index))
                return;

            _stageIndex.Value = index;
        }

        private void OnStageSelected(int stage)
        {
            var stages = _db.stages;
            if (!stages.TryFindIndex(stage, out int index))
                index = 0;
            
            if (!stages.TryIndexOf(index, out var entity))
                return;

            _ui.Set(entity);

            _prev.gameObject.SetActive(index > 0);
            _next.gameObject.SetActive(index < stages.Length - 1);
        }

        #region UI Events.

        private void OnClickSelect()
        {
            var message = new Result { selectedStage = _repository.stage.Value };
            message.Publish();
            
            Close();
        }

        private void OnPrevStage() => --_stageIndex.Value;

        private void OnNextStage() => ++_stageIndex.Value;

        #endregion

        public void Dispose()
        {
            _ui?.Dispose();
            _subscription?.Dispose();
        }

        private void Close()
        {
            gameObject.SetActive(false);
            Dispose();
        }
    }
}