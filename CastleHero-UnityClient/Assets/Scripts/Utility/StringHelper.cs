using UnityEngine;

namespace RGLabs.Utility
{
    public static class StringHelper
    {
        private const string ColoredStringTag = "<color={0}>{1}</color>";
        
        public static readonly Color PositiveColor = Color.green;
        public static readonly Color NegativeColor = Color.red;

        public static string CurrencyText(this int value) => value.ToString("N0");
        
        public static string WithPositiveColor(this string text) => text.WithColor(PositiveColor);
        public static string WithNegativeColor(this string text) => text.WithColor(NegativeColor);
        
        public static string WithColor(this string text, Color color) => string.Format(ColoredStringTag, color.Hex(), text);

        private static string Hex(this Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255);
            int g = Mathf.RoundToInt(color.g * 255);
            int b = Mathf.RoundToInt(color.b * 255);
            int a = Mathf.RoundToInt(color.a * 255);
            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        }
    }
}