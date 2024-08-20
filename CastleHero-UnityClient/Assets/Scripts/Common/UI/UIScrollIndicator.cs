using NaughtyAttributes;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace RGLabs.Common.UI
{
    public class UIScrollIndicator : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scroll;
        [MinMaxSlider(0f, 1f)]
        [SerializeField] private float _threshold;
        [SerializeField] private Button _button;

        private void Awake()
        {
            this.SubscribeButton(_button, OnClick);
        }

        private void OnClick()
        {
            if (_scroll.horizontal)
                _scroll.horizontalNormalizedPosition = 1f;
            else
                _scroll.verticalNormalizedPosition = 1f;
        }

        private void Update()
        {
            float normalized = 1f - (_scroll.horizontal
                ? _scroll.horizontalNormalizedPosition
                : _scroll.verticalNormalizedPosition);
            
            _button.gameObject.SetActive(normalized > _threshold);
        }
    }
}