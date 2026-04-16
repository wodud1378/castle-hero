using Cysharp.Threading.Tasks;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Lobby.Prepare.UI
{
    public class UIGameInfo : MonoBehaviour
    {
        [FormerlySerializedAs("_id")]
        [SerializeField] private TMP_Text id;
        [FormerlySerializedAs("_ap")]
        [SerializeField] private TMP_Text ap;
        [FormerlySerializedAs("_rewardList")]
        [SerializeField] private UIRewardList rewardList;

        private readonly ReactiveProperty<IGameEntity> _entity = new();

        private void Awake()
        {
            ServiceLocator.Get<IUserRepository>().Entrance
                .ThrottleFrame(1)
                .Subscribe(OnEntranceChanged)
                .AddTo(this);

            ServiceLocator.Get<IUserRepository>().Stamina.Point
                .ThrottleFrame(1)
                .Subscribe(OnApChanged)
                .AddTo(this);
        }

        private void OnApChanged(int apValue)
        {
            if (ap == null)
                return;

            if (_entity.Value == null)
                return;

            int require = _entity.Value.Ap;
            string text = $"-{require}";
            ap.text = apValue < require
                ? text.WithNegativeColor()
                : text.WithColor(Color.white);
        }

        private void OnEntranceChanged(GameEntrance entrance)
        {
            if (!ServiceLocator.Get<IDBProvider>().TryLoadGameEntity(entrance.type, entrance.id, out var entity))
                return;

            _entity.Value = entity;

            if (id != null)
            {
                int lv = entity.Lv;
                id.text = entity.Type switch
                {
                    GameType.Stage => $"STAGE {lv}",
                    GameType.Dungeon => $"LV {lv}",
                    _ => string.Empty
                };
            }

            if (rewardList != null)
                rewardList.Init(entity).Forget();

            OnApChanged(ServiceLocator.Get<IUserRepository>().Stamina.Point.Value);
        }
    }
}
