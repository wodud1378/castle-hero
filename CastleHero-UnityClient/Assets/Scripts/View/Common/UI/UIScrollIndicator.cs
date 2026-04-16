using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace CastleHero.View.Common.UI
{
    public class UIScrollIndicator : MonoBehaviour
    {
        [FormerlySerializedAs("_scroll")]
        [SerializeField] private ScrollRect scroll;
        [FormerlySerializedAs("_threshold")]
        [SerializeField] private float threshold;
        [FormerlySerializedAs("_button")]
        [SerializeField] private Button button;

        private void Awake()
        {
            this.SubscribeButton(button, OnClick);
        }

        private void OnClick()
        {
            if (scroll.horizontal)
                scroll.horizontalNormalizedPosition = 0f;
            else
                scroll.verticalNormalizedPosition = 0f;
        }

        private void Update()
        {
            float normalized = (scroll.horizontal
                ? scroll.horizontalNormalizedPosition
                : scroll.verticalNormalizedPosition);

            button.gameObject.SetActive(normalized > threshold);
        }
    }
}
