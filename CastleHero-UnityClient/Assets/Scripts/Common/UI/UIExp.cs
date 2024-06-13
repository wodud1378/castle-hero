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

        public void Set(int current, int next)
        {
            _value.text = $"{current}/{next}";

            float ratio = (float)current / next;
            _gauge.value = ratio;
            _percentage.text = $"{ratio * 100f:F1}";
        }
    }
}