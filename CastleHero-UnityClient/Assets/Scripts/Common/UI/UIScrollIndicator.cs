using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace RGLabs.Common.UI
{
    public class UIScrollIndicator : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private float _threshold;
        [SerializeField] private Button _button;

        private void Awake()
        {
            this.SubscribeButton(_button, OnClick);
        }

        private void OnClick()
        {
            if (_scroll.horizontal)
                _scroll.horizontalNormalizedPosition = 0f;
            else
                _scroll.verticalNormalizedPosition = 0f;
        }

        private void Update()
        {
            float normalized = (_scroll.horizontal
                ? _scroll.horizontalNormalizedPosition
                : _scroll.verticalNormalizedPosition);
            
            _button.gameObject.SetActive(normalized > _threshold);
        }
    }
}