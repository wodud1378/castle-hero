using RGLabs.Data;
using UnityEngine;

namespace RGLabs.Common.Localize
{
    public static class LocalizeHelper
    {
        public static string Localize(this int id) => Storage.localize.Get(id);

        public static SystemLanguage IsoToSystemLanguage(string code)
        {
            switch (code)
            {
                case "KO" : return SystemLanguage.Korean;
                case "JP" : return SystemLanguage.Japanese;
                case "CN" : return SystemLanguage.Chinese;
                case "TW" : return SystemLanguage.ChineseTraditional;
                default:
                    return SystemLanguage.English;
            }
        }
        public static string SystemLanguageToIso(SystemLanguage lang)
        {
            switch (lang)
            {
                case SystemLanguage.Korean : return "KO";
                case SystemLanguage.Japanese : return "JP";
                case SystemLanguage.Chinese : return "CN";
                case SystemLanguage.ChineseTraditional : return "TW";
                default:
                    return "EN";
            }
        }
    }
}