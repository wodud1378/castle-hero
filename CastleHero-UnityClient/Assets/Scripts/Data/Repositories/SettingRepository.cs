using System;
using UniRx;
using UnityEngine;

namespace RGLabs.Data.Repositories
{
    public class SettingRepository : IDisposable
    {
        public readonly BoolReactiveProperty bgmToggle = new(true);
        public readonly BoolReactiveProperty fxToggle = new(true);
        
        public readonly ReactiveProperty<float> bgmLevel = new(1f);
        public readonly ReactiveProperty<float> fxLevel = new(1f);
        
        public readonly ReactiveProperty<SystemLanguage> language;

        private const string LanguageKey = "lang";

        public SettingRepository()
        {
            var saved = PlayerPrefs.GetString(LanguageKey, string.Empty);
            var defaultLang = string.IsNullOrEmpty(saved)
                ? Application.systemLanguage
                : Enum.TryParse(typeof(SystemLanguage), saved, out var value)
                    ? (SystemLanguage)value
                    : Application.systemLanguage;

            language = new ReactiveProperty<SystemLanguage>(defaultLang);
            language
                .ThrottleFrame(1)
                .Subscribe(x =>
                {
                    PlayerPrefs.SetString(LanguageKey, x.ToString());
                });
        }

        public void Dispose()
        {
            bgmToggle?.Dispose();
            fxToggle?.Dispose();
            bgmLevel?.Dispose();
            fxLevel?.Dispose();
            language?.Dispose();
        }
    }
}