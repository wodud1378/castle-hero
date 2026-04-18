using UnityEngine;
using CastleHero.Common.Pattern;

namespace CastleHero.Common.Localize
{
    public static class LocalizeHelper
    {
        // Bootstrap (BackendBootService) 에서 등록한 LocalizeText 를 한 번만 캐싱.
        // ServiceLocator.Instance.OnRegistered 구독으로 자동 갱신 (교체 시에도 안전).
        private static LocalizeText _cached;

        static LocalizeHelper()
        {
            if (ServiceLocator.Instance.TryGet<LocalizeText>(out var lt))
                _cached = lt;

            ServiceLocator.Instance.OnRegistered += (type, instance) =>
            {
                if (type == typeof(LocalizeText))
                    _cached = (LocalizeText)instance;
            };
        }

        public static string Localize(this int id) => _cached != null ? _cached.Get(id) : string.Empty;

        public static SystemLanguage IsoToSystemLanguage(string code)
        {
            switch (code)
            {
                case "KO": return SystemLanguage.Korean;
                case "JP": return SystemLanguage.Japanese;
                case "CN": return SystemLanguage.Chinese;
                case "TW": return SystemLanguage.ChineseTraditional;
                default:
                    return SystemLanguage.English;
            }
        }

        public static string SystemLanguageToIso(SystemLanguage lang)
        {
            switch (lang)
            {
                case SystemLanguage.Korean: return "KO";
                case SystemLanguage.Japanese: return "JP";
                case SystemLanguage.Chinese: return "CN";
                case SystemLanguage.ChineseTraditional: return "TW";
                default:
                    return "EN";
            }
        }
    }
}
