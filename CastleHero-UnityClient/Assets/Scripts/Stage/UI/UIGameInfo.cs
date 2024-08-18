using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace RGLabs.Stage.UI
{
    public class UIGameInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text _id;
        [SerializeField] private TMP_Text _ap;
        [SerializeField] private UIRewardList _rewardList;

        private readonly ReactiveProperty<IGameEntity> _entity = new();

        private void Awake()
        {
            Storage.userRepository.gameEntrance
                .ThrottleFrame(1)
                .Subscribe(OnEntranceChanged)
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

            int require = _entity.Value.Ap;
            string text = $"-{require}";
            _ap.text = ap < require
                ? text.WithNegativeColor()
                : text.WithColor(Color.white);
        }

        private void OnEntranceChanged(GameEntrance entrance)
        {
            if (!Storage.db.TryLoadGameEntity(entrance.type, entrance.id, out var entity))
                return;

            _entity.Value = entity;

            if (_id != null)
            {
                int lv = entity.Lv;
                _id.text = entity.Type switch
                {
                    GameType.Stage => $"STAGE {lv}",
                    GameType.Dungeon => $"LV {lv}",
                    _ => string.Empty
                };
            }
            
            if(_rewardList != null)
                _rewardList.Init(entity).Forget();

            OnApChanged(Storage.userRepository.act.point.Value);
        }
    }
}