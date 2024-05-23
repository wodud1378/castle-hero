using System;
using RGLabs.Unit.Behaviours;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Skill.Components
{
    public interface ICycle
    {
        public enum Option
        {
            CoolTime
        }
        
        public UnitBehaviour Owner { get; set; }
        public bool IsReady { get; }

        public void StartWaiting(Action onEnd = null);
    }

    public class Timer
    {
        private readonly UnitBehaviour _owner;
        private readonly float _coolTime;

        public float leftTime;

        private IDisposable _disposable;
        private Action _onEnd;
        
        public Timer(UnitBehaviour owner, float coolTime)
        {
            _owner = owner;
            _coolTime = coolTime;
        }

        public void Run(Action onEnd = null)
        {
            leftTime = _coolTime;

            _onEnd = onEnd;
            _disposable = _owner
                .UpdateAsObservable()
                .Select(_=> Time.deltaTime)
                .Subscribe(Update)
                .AddTo(_owner);
        }

        public void Stop()
        {
            if (leftTime <= 0f)
            {
                _onEnd?.Invoke();
                _onEnd = null;
            }
            
            _disposable?.Dispose();
        }

        private void Update(float deltaTime)
        {
            leftTime -= deltaTime;
            if (leftTime > 0f)
                return;
            
            Stop();
        }
    }
    
    public class CoolTime : ICycle
    {
        public UnitBehaviour Owner { get; set; }
        public bool IsReady { get; private set; }
        
        private readonly Timer _timer;

        public CoolTime(UnitBehaviour owner, float coolTime)
        {
            _timer = new(owner, coolTime);

            Owner = owner;
        }

        public void StartWaiting(Action onEnd = null)
        {
            IsReady = false;
            _timer.Run(() =>
            {
                IsReady = true;
                onEnd?.Invoke();
            });
        }
    }
}