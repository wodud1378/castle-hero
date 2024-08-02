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
            _level.Set(data.lvTransition[0], data.expTransition[0]);


            return _unitSlot.Init(data.unit);
        }

        public void Show() => StartCoroutine(Play(_data.lvTransition, _data.expTransition));

        private IEnumerator Play(int[] lvTransition, int[] expTransition)
        {
            int lv = lvTransition[0];
            int currentLv = lvTransition[1];
        
            int exp = expTransition[0];
            int currentExp = expTransition[1];
            
            _level.ReleaseOverride();
            _level.Set(lv, exp);
            
            _addedExp.DOFade(1f, 0.25f)
                .From(0f)
                .OnComplete(() =>
                {
                    _addedExp.DOFade(0.5f, 0.5f)
                        .From(1f)
                        .SetLoops(-1, LoopType.Yoyo);
                });
        
            float totalExp = 0;
            for (int l = lv; l <= currentLv; ++l)
            {
                Storage.db.levels.TryFind(l, out var entity);
        
                if (l == lv)
                    totalExp += entity.exp - exp;
                else if (l != currentLv)
                    totalExp += entity.exp;
                else
                    totalExp += currentExp;
            }

            float perFrame = totalExp / (_expDuration * 60f);
            bool updatedNext = false;
            int nextExp = 0;
            while (lv <= currentLv && exp < currentExp)
            {
                if (!updatedNext)
                {
                    nextExp = lv == currentLv
                        ? Storage.db.levels.TryFind(lv, out var e)
                            ? e.exp
                            : 0
                        : currentExp;
                    
                    updatedNext = true;
                }

                exp = Mathf.Clamp(exp + (int)perFrame, exp, nextExp);

                _addedExp.text = exp.ToString();
                _level.SetOverride(lv, exp);

                if (exp >= nextExp)
                {
                    exp = 0;
                    updatedNext = false;
                    ++lv;
                }
                
                yield return null;
            }
        }

#if UNITY_EDITOR
        public int[] lvs;
        public int[] exps;
        public bool show;
        
        private void Update()
        {
            if (show)
            {
                show = false;
                StartCoroutine(Play(lvs, exps));
            }
        }
#endif
    }
}