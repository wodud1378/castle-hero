using System.Text;
using CastleHero.GamePlay.Unit;
using CastleHero.Utility;
using TMPro;
using UnityEngine;

namespace CastleHero.View.Lobby.UI
{
    public class UIStatusText : MonoBehaviour
    {
        public Status.Type type;
        public TMP_Text label;

        public void SetText(float baseValue, float additionalValue = 0f)
        {
            string baseText;
            string additionalText;
            string suffix = string.Empty;
            if (type is not Status.Type.Hp and not Status.Type.Atk)
            {
                baseValue *= 100f;
                additionalValue *= 100f;
            }

            switch (type)
            {
                case Status.Type.Hp:
                case Status.Type.Atk:
                    baseText = $"{baseValue:N0}";
                    additionalText = additionalValue != 0f ? $"{additionalValue:N0}" : string.Empty;
                    break;
                case Status.Type.Critical:
                case Status.Type.CriticalAtk:
                    baseText = $"{baseValue:#,##0.#}";
                    additionalText = additionalValue != 0f ? $"{additionalValue:#,##0.#}" : string.Empty;
                    suffix = "%";
                    break;
                default:
                    baseText = $"{baseValue:#,##0.##}";
                    additionalText = additionalValue != 0f ? $"{additionalValue:#,##0.##}" : string.Empty;
                    break;
            }

            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(additionalText))
            {
                bool isPositive = additionalValue > 0f;
                var sign = isPositive ? "+" : string.Empty;
                var str = $"({sign}{additionalText}{suffix})";
                str = isPositive ? str.WithPositiveColor() : str.WithNegativeColor();
                builder.Append($"{str} ");
            }

            builder.Append($"{baseText}{suffix}");

            label.text = builder.ToString();
        }

        private void OnValidate()
        {
            if (label == null)
                label = GetComponentInChildren<TMP_Text>();
        }
    }
}
