using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace CastleHero.Data.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        public BoolReactiveProperty bgmToggle { get; } = new(PlayerPrefs.GetInt(BgmToggleKey, 1) == 1);
        public BoolReactiveProperty fxToggle { get; } = new(PlayerPrefs.GetInt(FxToggleKey, 1) == 1);

        public ReactiveProperty<float> bgmLevel { get; } = new(PlayerPrefs.GetFloat(BgmLevelKey, 1f));
        public ReactiveProperty<float> fxLevel { get; } = new(PlayerPrefs.GetFloat(FxLevelKey, 1f));

        public BoolReactiveProperty speedUp { get; } = new(PlayerPrefs.GetInt(SpeedUpKey, 0) == 1);
        public BoolReactiveProperty repeat { get; } = new(PlayerPrefs.GetInt(RepeatKey, 0) == 1);

        public ReactiveProperty<SystemLanguage> language { get; }

        private const string LanguageKey = "lang";

        private const string BgmToggleKey = "toggle-bgm";
        private const string FxToggleKey = "toggle-fx";

        private const string BgmLevelKey = "level-bgm";
        private const string FxLevelKey = "level-fx";

        private const string SpeedUpKey = "speed-up";
        private const string RepeatKey = "repeat";

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
            _subscriptions.Add(speedUp.Subscribe(x => PlayerPrefs.SetInt(SpeedUpKey, x ? 1 : 0)));
            _subscriptions.Add(repeat.Subscribe(x => PlayerPrefs.SetInt(RepeatKey, x ? 1 : 0)));
        }

        public void Dispose()
        {
            bgmToggle?.Dispose();
            fxToggle?.Dispose();
            bgmLevel?.Dispose();
            fxLevel?.Dispose();
            speedUp?.Dispose();
            repeat?.Dispose();
            language?.Dispose();

            _subscriptions.ForEach(x => x.Dispose());
        }
    }
}