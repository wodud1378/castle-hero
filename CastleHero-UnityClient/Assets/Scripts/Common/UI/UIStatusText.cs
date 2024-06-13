using System;
using System.Text;
using RGLabs.Unit;
using RGLabs.Utility;
using TMPro;
using UnityEngine;

namespace RGLabs.Common.UI
{
    public class UIStatusText : MonoBehaviour
    {
        [SerializeField] private string _suffix;

        public Status.Type type;
        public TMP_Text label;

        public void SetText(float baseValue, float additionalValue = 0f)
        {
            float multiplier = type switch
            {
                Status.Type.Critical or Status.Type.CriticalAtk => 100f,
                _ => 1f,
            };

            baseValue *= multiplier;
            additionalValue *= multiplier;

            var builder = new StringBuilder();
            if (additionalValue != 0f)
            {
                var str = $"{additionalValue}{_suffix}";
                str = additionalValue > 0f ? str.WithPositiveColor() : str.WithNegativeColor();
                builder.Append(str);
            }

            builder
                .Append(baseValue)
                .Append(_suffix);

            label.text = builder.ToString();
        }

        private void OnValidate()
        {
            if (label == null)
                label = GetComponentInChildren<TMP_Text>();
        }
    }
}