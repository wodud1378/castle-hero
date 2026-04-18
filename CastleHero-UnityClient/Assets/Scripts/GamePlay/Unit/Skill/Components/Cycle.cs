using System;
using CastleHero.GamePlay.Unit.Behaviours;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Skill.Components
{
    public interface ICycle
    {
        public enum Option
        {
            CoolTime
        }
        
        public UnitActor Owner { get; set; }
        public bool IsReady { get; }

        public void StartWaiting(Action onEnd = null);
    }

    public class Timer
    {
        public readonly float time;
        
        private readonly UnitActor _owner;

        public float leftTime;

        private IDisposable _disposable;
        private Action _onEnd;
        
        public Timer(UnitActor owner, float time)
        {
            _owner = owner;
            this.time = time;
        }

        public void Run(Action onEnd = null)
        {
            leftTime = time;

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
        public UnitActor Owner { get; set; }
        public bool IsReady { get; private set; }
        
        public readonly Timer timer;

        public CoolTime(UnitActor owner, float coolTime)
        {
            timer = new(owner, coolTime);

            Owner = owner;
        }

        public void StartWaiting(Action onEnd = null)
        {
            IsReady = false;
            timer.Run(() =>
            {
                IsReady = true;
                onEnd?.Invoke();
            });
        }
    }
}