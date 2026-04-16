using System;
using UniRx;
using UnityEngine;

namespace CastleHero.Data.Repositories
{
    public interface ISettingRepository : IDisposable
    {
        BoolReactiveProperty bgmToggle { get; }
        BoolReactiveProperty fxToggle { get; }
        ReactiveProperty<float> bgmLevel { get; }
        ReactiveProperty<float> fxLevel { get; }
        BoolReactiveProperty speedUp { get; }
        BoolReactiveProperty repeat { get; }
        ReactiveProperty<SystemLanguage> language { get; }
    }
}
