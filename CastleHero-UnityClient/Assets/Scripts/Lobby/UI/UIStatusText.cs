using System.Text;
using RGLabs.Unit;
using RGLabs.Utility;
using TMPro;
using UnityEngine;

namespace RGLabs.Lobby.UI
{
    public class UIStatusText : TMP_Text
    {
        private const string ColoredString = "<color={0}>{1}</color>";
        
        [SerializeField] private string _suffix;
        [SerializeField] private Color _increaseColor;
        [SerializeField] private Color _decreasedColor;

        public Status.Type type;
        
        public void SetText(float baseValue, float additionalValue = 0f)
        {
            var builder = new StringBuilder();
            if (additionalValue != 0f)
            {
                var hex = (additionalValue > 0f ? _increaseColor : _decreasedColor).Hex();
                var additionalString = $"{additionalValue}{_suffix}";

                builder.Append(string.Format(ColoredString, hex, additionalString));
            }

            builder
                .Append(baseValue)
                .Append(_suffix);

            text = builder.ToString();
        }
    }
}