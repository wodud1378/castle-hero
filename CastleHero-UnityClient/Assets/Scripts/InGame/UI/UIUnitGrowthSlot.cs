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

        public void Show() => Play();

        private void Play()
        {
            int lv = _data.lvTransition[0];
            int currentLv = _data.lvTransition[1];

            int exp = _data.expTransition[0];
            int currentExp = _data.expTransition[1];
            
            _level.Set(lv, exp);

            float timePerOnce = _expDuration / (currentLv - lv + 1);
            var seq = DOTween.Sequence();
            
            int totalExp = 0;
            // Exp Gauge.
            for (; lv <= currentLv; ++lv)
            {
                if (!Storage.db.levels.TryFind(lv, out var data))
                    continue;

                totalExp += lv < currentLv ? data.exp : currentExp;

                int endExp = (lv == currentLv) ? currentExp : data.exp;

                seq.Append(DOTween.To(() => exp, x => exp = x, endExp, timePerOnce)
                    .OnUpdate(() =>
                    {
                        _level.SetOverride(lv, exp);
                    })
                    .OnComplete(() =>
                    {
                        exp = lv < currentLv ? 0 : currentExp;
                    }));
            }

            _addedExp.text = $"{totalExp}";
            _addedExp.DOFade(1f, 0.25f)
                .From(0f)
                .OnComplete(() =>
                {
                    _addedExp.DOFade(0.75f, 0.15f)
                        .From(1f)
                        .SetLoops(-1, LoopType.Yoyo);
                });
            
            seq.Play();
        }
    }
}