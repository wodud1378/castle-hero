using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace RGLabs.Stage.UI
{
    public class UIStageInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text _stage;
        [SerializeField] private TMP_Text _ap;
        [SerializeField] private UIStageRewardList _rewardList;

        private readonly ReactiveProperty<StageEntity> _entity = new();

        private void Awake()
        {
            Storage.userRepository.profile.focusedStage
                .ThrottleFrame(1)
                .Subscribe(OnStageChanged)
                .AddTo(this);

            Storage.userRepository.act.point
                .ThrottleFrame(1)
                .Subscribe(OnApChanged)
                .AddTo(this);
        }

        private void OnApChanged(int ap)
        {
            if (_ap == null)
                return;

            if (!_entity.Value.IsValid)
                return;

            int require = _entity.Value.ap;
            string text = $"-{require}";
            _ap.text = ap < require
                ? text.WithNegativeColor()
                : text.WithColor(Color.white);
        }

        private void OnStageChanged(int stage)
        {
            if (!Storage.db.stages.TryFind(stage, out var entity))
                return;

            _entity.Value = entity;
            
            if(_stage != null)
                _stage.text = $"STAGE {stage}";
            
            if(_rewardList != null)
                _rewardList.Init(entity).Forget();

            OnApChanged(Storage.userRepository.act.point.Value);
        }
    }
}