using System;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using CastleHero.Data;
using CastleHero.GamePlay.InGame.Behaviours;
using CastleHero.View.InGame.Behaviours;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using CastleHero.GamePlay.InGame;

namespace CastleHero.View.InGame.UI
{
    public class UICastleState : MonoBehaviour
    {
        [FormerlySerializedAs("_lv")]
        [SerializeField] private TMP_Text lv;
        [FormerlySerializedAs("_hp")]
        [SerializeField] private TMP_Text hp;
        [FormerlySerializedAs("_hpBar")]
        [SerializeField] private Slider hpBar;
        [FormerlySerializedAs("_root")]
        [SerializeField] private GameObject root;

        private UnitActor _castle;

        private void Awake()
        {
            root.SetActive(false);

            InGameSession.Current.Castle
                .Subscribe(x => OnCastleChanged(x as UnitActor))
                .AddTo(this);

            this.SubscribeMessage<ExitGame>(_ => root.SetActive(false));
        }

        private void OnCastleChanged(UnitActor castle)
        {
            _castle = castle;
            if (_castle == null)
            {
                root.SetActive(false);
                return;
            }

            root.SetActive(true);
            _castle.Information
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    lv.text = $"Lv.{x.lv}";
                })
                .AddTo(_castle);

            var hpStatus = _castle.Status.hp;
            _castle
                .UpdateAsObservable()
                .Subscribe(_ =>
                {
                    float left = hpStatus.Left;
                    float max = hpStatus.Max;
                    float ratio = left / max;

                    hpBar.value = ratio;
                    hp.text = $"{(int)left:N0}/{(int)max:N0}";
                })
                .AddTo(_castle);
        }
    }
}
