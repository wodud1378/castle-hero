using System;
using UniRx;
using UnityEngine;

namespace RGLabs.Common
{
    public class Timer : IDisposable
    {
        public event Action OnFinished;
        
        public readonly ReactiveProperty<double> leftTime = new();

        private IDisposable _update;

        public void Run(double time)
        {
            Stop();

            leftTime.Value = time;
            _update = Observable
                .EveryUpdate()
                .Select(_=> Time.deltaTime)
                .Subscribe(x =>
                {
                    leftTime.Value -= x;

                    if (leftTime.Value > 0)
                        return;
                    
                    Stop();
                    
                    OnFinished?.Invoke();
                });
        }

        public void Stop() => _update?.Dispose();

        public void Dispose()
        {
            leftTime?.Dispose();
            _update?.Dispose();
            OnFinished = null;
        }
    }
}