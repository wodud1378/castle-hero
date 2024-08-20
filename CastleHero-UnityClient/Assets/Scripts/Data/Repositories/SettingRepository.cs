using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace RGLabs.Data.Repositories
{
    public class SettingRepository : IDisposable
    {
        public readonly BoolReactiveProperty bgmToggle = new(PlayerPrefs.GetInt(BgmToggleKey, 1) == 1);
        public readonly BoolReactiveProperty fxToggle = new(PlayerPrefs.GetInt(FxToggleKey, 1) == 1);

        public readonly ReactiveProperty<float> bgmLevel = new(PlayerPrefs.GetFloat(BgmLevelKey, 1f));
        public readonly ReactiveProperty<float> fxLevel =  new(PlayerPrefs.GetFloat(FxLevelKey, 1f));
        
        public readonly ReactiveProperty<SystemLanguage> language;

        private const string LanguageKey = "lang";
        
        private const string BgmToggleKey = "toggle-bgm";
        private const string FxToggleKey = "toggle-fx";

        private const string BgmLevelKey = "level-bgm";
        private const string FxLevelKey = "level-bgm";
        
        private readonly List<IDisposable> _subscriptions = new();

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

            _subscriptions.Add(bgmToggle.Subscribe(x => PlayerPrefs.SetInt(BgmToggleKey, x ? 1 : 0)));
            _subscriptions.Add(fxToggle.Subscribe(x => PlayerPrefs.SetInt(FxToggleKey, x ? 1 : 0)));
            _subscriptions.Add(bgmLevel.Subscribe(x => PlayerPrefs.SetFloat(BgmLevelKey, x)));
            _subscriptions.Add(fxLevel.Subscribe(x => PlayerPrefs.SetFloat(FxLevelKey, x)));
        }

        public void Dispose()
        {
            bgmToggle?.Dispose();
            fxToggle?.Dispose();
            bgmLevel?.Dispose();
            fxLevel?.Dispose();
            language?.Dispose();
            
            _subscriptions.ForEach(x => x.Dispose());
        }
    }
}