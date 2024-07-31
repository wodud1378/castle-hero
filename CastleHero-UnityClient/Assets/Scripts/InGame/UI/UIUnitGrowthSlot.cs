using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Network.Shared;
using TMPro;
using UnityEngine;

namespace RGLabs.InGame.UI
{
    public class UIUnitGrowthSlot : UISlot
    {
        [SerializeField] private float _expDuration = 1.5f;
        [SerializeField] private UILevel _level;
        [SerializeField] private TMP_Text _addedExp;
        [SerializeField] private UICharacterSlot _unitSlot;

        private UnitTransition _data;

        public UniTask Init(UnitTransition data)
        {
            _data = data;

            return _unitSlot.Init(data.unit);
        }

        public void Show() => Play(_data.lvTransition, _data.expTransition);

        // private IEnumerator P(int[] lvTransition, int[] expTransition)
        // {
        //     int lv = lvTransition[0];
        //     int currentLv = lvTransition[1];
        //
        //     int exp = expTransition[0];
        //     int currentExp = expTransition[1];
        //     
        //     _level.Set(lv, exp);
        //
        //     int totalExp = 0;
        //     for (int l = lv; l <= currentLv; ++l)
        //     {
        //         Storage.db.levels.TryFind(l, out var entity);
        //
        //         if (l == lv)
        //             totalExp += entity.exp - exp;
        //         else if (l != currentLv)
        //             totalExp += entity.exp;
        //         else
        //             totalExp += currentExp;
        //     }
        //
        //     while (lv < currentLv && exp < currentExp)
        //     {
        //     }
        // }

        private void Play(int[] lvTransition, int[] expTransition)
        {
            int lv = lvTransition[0];
            int currentLv = lvTransition[1];

            int exp = expTransition[0];
            int currentExp = expTransition[1];

            _level.Set(lv, exp);

            float timePerOnce = _expDuration / (currentLv - lv + 1);
            var seq = DOTween.Sequence();

            int totalExp = 0;
            // Exp Gauge.
            for (; lv <= currentLv; ++lv)
            {
                int forNext = GetExpForNext(lv);

                totalExp += lv < currentLv ? forNext : currentExp;

                int endExp = (lv == currentLv) ? currentExp : forNext;

                seq.Append(DOTween.To(() => exp, x => exp = x, endExp, timePerOnce)
                    .OnUpdate(() => { _level.SetOverride(lv, exp); })
                    .OnComplete(() => { exp = lv < currentLv ? 0 : currentExp; }));
            }

            _addedExp.text = $"{totalExp}";
            _addedExp.DOFade(1f, 0.25f)
                .From(0f)
                .OnComplete(() =>
                {
                    _addedExp.DOFade(0.5f, 0.5f)
                        .From(1f)
                        .SetLoops(-1, LoopType.Yoyo);
                });

            seq.Play();
        }

        private int GetExpForNext(int lv)
        {
            if (!Storage.db.levels.TryFind(lv, out var data))
                return 0;

            return data.exp;
        }

#if UNITY_EDITOR
        public int[] lvs;
        public int[] exps;
        public bool show;
        
        protected override void OnValidate()
        {
            base.OnValidate();

            if (show)
            {
                show = false;
                
                int lv = lvs[0];
                int currentLv = lvs[1];

                int exp = exps[0];
                int currentExp = exps[1];

                _level.ReleaseOverride();
                _level.Set(lv, exp);

                float timePerOnce = _expDuration / (currentLv - lv + 1);
                var seq = DOTween.Sequence();

                int totalExp = 0;
                // Exp Gauge.
                for (; lv <= currentLv; ++lv)
                {
                    int forNext = lv * 200;

                    totalExp += lv < currentLv ? forNext : currentExp;

                    int endExp = (lv == currentLv) ? currentExp : forNext;

                    int l = lv;
                    seq.Append(DOTween.To(() => exp, x => exp = x, endExp, timePerOnce)
                        .OnUpdate(() =>
                        {
                            _level.SetOverride(l, exp);
                        })
                        .OnComplete(() => { exp = l < currentLv ? 0 : currentExp; }));
                }

                _addedExp.text = $"{totalExp}";
                _addedExp.DOFade(1f, 0.25f)
                    .From(0f)
                    .OnComplete(() =>
                    {
                        _addedExp.DOFade(0.5f, 0.5f)
                            .From(1f)
                            .SetLoops(-1, LoopType.Yoyo);
                    });

                seq.Play();
            }
        }
#endif
    }
}