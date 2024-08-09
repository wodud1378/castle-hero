using System;
using RGLabs.Data;
using RGLabs.InGame.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame
{
    public struct TimeOver
    {
    }

    public class StageTimer : IDisposable
    {
        private readonly IDisposable _update;
        private readonly IDisposable _subscription;

        private bool _disposed;

        public StageTimer()
        {
            var leftTime = Storage.inGameRepository.leftTime;

            _update = Observable
                .EveryUpdate()
                .Select(_ => Time.deltaTime)
                .Subscribe(x =>
                {
                    leftTime.Value -= x;

                    if (leftTime.Value > 0)
                        return;

                    new TimeOver().Publish();
                    Dispose();
                });

            _subscription = MessageBroker.Default
                .Receive<GameResult>()
                .Subscribe(_ => Dispose());
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _update?.Dispose();
            _subscription?.Dispose();
            _disposed = true;
        }
    }
}