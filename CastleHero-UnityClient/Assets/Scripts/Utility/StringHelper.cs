using System;
using System.Text.RegularExpressions;
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
        
        public static string WithColor(this string text, Color color)
        {
            var stripped = Regex.Replace(text, @"<color=.*?>|</color>", string.Empty); 
            
            return string.Format(ColoredStringTag, color.Hex(), stripped);
        }

        public static string ToLeftTimeForResetText(this double seconds)
        {
            int totalSeconds = (int)seconds;
            int days = totalSeconds / 86400;
            int hours = (totalSeconds % 86400) / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int secs = totalSeconds % 60;
            
            return days > 0 
                ? $"{days}d:{hours:D2}h:{minutes:D2}m:{secs:D2}s" 
                : $"{hours:D2}h:{minutes:D2}m:{secs:D2}s";
        }

        public static string ToLeftTimeForExpireText(this double seconds)
        {
            int totalSeconds = (int)seconds;
            int days = totalSeconds / 86400;
            if (days > 0)
            {
                return $"{days}일 후 만료";
            }
            
            int hours = (totalSeconds % 86400) / 3600;
            if (hours > 0)
            {
                return $"{hours}시간 후 만료";
            }
            
            int minutes = (totalSeconds % 3600) / 60;
            if (minutes > 0)
            {
                return $"{minutes}분 후 만료";
            }
            
            int secs = totalSeconds % 60;
            if (secs > 0)
            {
                return $"{secs}초 후 만료";
            }

            return string.Empty;
        }

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