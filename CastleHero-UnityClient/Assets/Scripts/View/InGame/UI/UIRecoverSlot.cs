using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.Unit.Behaviours;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CastleHero.View.InGame.UI
{
    public class UIRecoverSlot : UISlot
    {
        [FormerlySerializedAs("_gauge")]
        [SerializeField] private Image gauge;

        public UniTask InitAsync(WaitRecover data)
        {
            data.summary
                .Subscribe(OnUpdate)
                .AddTo(this);

            return Init((data.behaviour as UnitBehaviour)?.Data.icon);
        }

        private void OnUpdate((float left, float total) summary) => gauge.fillAmount = summary.left / summary.total;
    }
}
