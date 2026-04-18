using System.Collections;
using DG.Tweening;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Network.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.InGame.UI
{
    public class UIUnitGrowthSlot : UISlot
    {
        [FormerlySerializedAs("_expDuration")]
        [SerializeField] private float expDuration = 1.5f;
        [FormerlySerializedAs("_level")]
        [SerializeField] private UILevel level;
        [FormerlySerializedAs("_addedExp")]
        [SerializeField] private TMP_Text addedExp;
        [FormerlySerializedAs("_unitSlot")]
        [SerializeField] private UICharacterSlot unitSlot;

        private UnitTransition _data;

        private IDBProvider _db;

        protected override void OnAwake()
        {
            base.OnAwake();

            _db = ServiceLocator.Instance.Get<IDBProvider>();
        }

        public void Init(UnitTransition data)
        {
            _data = data;
            addedExp.text = 0.ToString();
            level.Set(data.lvTransition[0], data.expTransition[0]);

            unitSlot.Init(data.unit);
        }

        public void Show() => StartCoroutine(Play(_data.lvTransition, _data.expTransition, _data.addedExp));

        private IEnumerator Play(int[] lvTransition, int[] expTransition, int addedExpValue)
        {
            int lv = lvTransition[0];
            int currentLv = lvTransition[1];

            int exp = expTransition[0];
            int currentExp = expTransition[1];

            level.ReleaseOverride();
            level.Set(lv, exp);

            // DOTween Free는 TMP_Text 전용 확장이 없어서 color alpha 를 직접 보간.
            DOTween.To(() => addedExp.color.a, v => { var c = addedExp.color; c.a = v; addedExp.color = c; }, 1f, 0.25f)
                .From(0f)
                .OnComplete(() =>
                {
                    DOTween.To(() => addedExp.color.a, v => { var c = addedExp.color; c.a = v; addedExp.color = c; }, 0.5f, 0.5f)
                        .From(1f)
                        .SetLoops(-1, LoopType.Yoyo);
                });

            float perFrame = addedExpValue / (expDuration * 60f);
            bool updatedNext = false;
            int nextExp = 0;
            while (lv <= currentLv && exp < currentExp)
            {
                if (!updatedNext)
                {
                    nextExp = lv == currentLv
                        ? _db.Levels.TryFind(lv, out var e)
                            ? e.exp
                            : 0
                        : currentExp;

                    updatedNext = true;
                }

                exp = Mathf.Clamp(exp + (int)perFrame, exp, nextExp);

                addedExp.text = exp.ToString();
                level.SetOverride(lv, exp);

                if (exp >= nextExp)
                {
                    exp = 0;
                    updatedNext = false;
                    ++lv;
                }

                yield return null;
            }

            addedExp.text = addedExpValue.ToString();
        }
    }
}
