using DG.Tweening;
using RGLabs.Data;
using RGLabs.Network.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UIExp : MonoBehaviour
    {
        [SerializeField] private Slider _gauge;
        [SerializeField] private TMP_Text _percentage;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private float _transitionTime;

        public void Set(UnitInfo info)
        {
            int current = info.exp;
            int next = !Storage.db.levels.TryFind(info.lv, out var entity) ? current : entity.exp;
            
            Set(current, next);
        }
        
        public void Set(int current, int next)
        {
            _value.text = $"{current}/{next}";

            float ratio = (float)current / next;
            _gauge.value = ratio;
            _percentage.text = $"{ratio * 100f:F1}";
        }

        private void Fill(float from, float to)
        {
            _percentage.DOFade(0f, 0.1f);
            //_gauge.image.DOFillAmount()
        }

        public void Transition(int count, float end)
        {
            
        }
    }
}