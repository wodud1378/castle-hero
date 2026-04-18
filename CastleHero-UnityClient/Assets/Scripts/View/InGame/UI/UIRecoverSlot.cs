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

        public void InitAsync(WaitRecover data)
        {
            data.summary
                .Subscribe(OnUpdate)
                .AddTo(this);

            Init((data.actor as UnitActor)?.Data.icon);
        }

        private void OnUpdate((float left, float total) summary) => gauge.fillAmount = summary.left / summary.total;
    }
}
