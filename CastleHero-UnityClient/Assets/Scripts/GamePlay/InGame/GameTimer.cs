using System;
using CastleHero.GamePlay.InGame.Behaviours;
using UniRx;
using UnityEngine;

namespace CastleHero.GamePlay.InGame
{
    public class GameTimer : IDisposable
    {
        public event Action OnTimeOver;
        
        private readonly IDisposable _update;
        private readonly IDisposable _subscription;

        private bool _onRun;
        private bool _disposed;

        public GameTimer(ReactiveProperty<float> timeProperty)
        {
            _update = Observable
                .EveryUpdate()
                .Select(_ => Time.deltaTime)
                .Subscribe(x =>
                {
                    if (!_onRun)
                        return;
                    
                    timeProperty.Value -= x;

                    if (timeProperty.Value > 0)
                        return;

                    OnTimeOver?.Invoke();
                    Dispose();
                });

            _subscription = MessageBroker.Default
                .Receive<GameResult>()
                .Subscribe(_ => Dispose());
        }

        public void Run() => _onRun = true;

        public void Dispose()
        {
            if (_disposed)
                return;

            _update?.Dispose();
            _subscription?.Dispose();
            _disposed = true;
            OnTimeOver = null;
        }
    }
}