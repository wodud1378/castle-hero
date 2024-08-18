using System;
using UniRx;
using UnityEngine;

namespace RGLabs.Common
{
    public class Timer : IDisposable
    {
        public readonly ReactiveProperty<double> leftTime = new();

        private IDisposable _update;

        public void Run(double time)
        {
            Stop();

            _update = Observable
                .EveryUpdate()
                .Select(_=> Time.deltaTime)
                .Subscribe(x =>
                {
                    leftTime.Value -= x;

                    if (leftTime.Value > 0)
                        return;
                    
                    Stop();
                });
        }

        public void Stop() => _update?.Dispose();

        public void Dispose()
        {
            leftTime?.Dispose();
            _update?.Dispose();
        }
    }
}