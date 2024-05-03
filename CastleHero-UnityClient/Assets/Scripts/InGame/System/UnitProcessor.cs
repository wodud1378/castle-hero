using System;
using System.Collections.Generic;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.System
{
    public struct AtkEvent
    {
        public UnitBehaviour from;
        public UnitBehaviour to;

        public float amount;
        public float critical;
        public float criticalMul;
    }
    
    public struct HealEvent
    {
        public UnitBehaviour from;
        public UnitBehaviour to;
        public float amount;
    }

    public struct AtkResult
    {
        public UnitBehaviour from;
        public UnitBehaviour to;
        public bool isCritical;
        public float amount;
    }

    public class UnitProcessor : IDisposable
    {
        private readonly List<IDisposable> _disposables;
        
        public UnitProcessor()
        {
            _disposables = new();
            _disposables.Add(MessageBroker.Default.Receive<AtkEvent>().Subscribe(OnReceiveAtkEvent));
            _disposables.Add(MessageBroker.Default.Receive<HealEvent>().Subscribe(OnReceiveHealEvent));
        }

        private void OnReceiveAtkEvent(AtkEvent ev)
        {
            var to = ev.to;
            if (!to.IsValid())
                return;

            float amount = CalcAmount(ev.amount, ev.critical, ev.criticalMul, out bool isCritical);
            to.status.hp.Decrease(amount);
            
            if(to.Hit != null)
                to.Hit.Play();

            new AtkResult
            {
                from = ev.from,
                to = ev.to,
                isCritical = isCritical,
                amount = amount
            }.Publish();
        }

        private void OnReceiveHealEvent(HealEvent ev)
        {
            var to = ev.to;
            if (!to.IsValid())
                return;

            to.status.hp.Increase(ev.amount);
        }

        private float CalcAmount(float atk, float critical, float criticalAtk, out bool isCritical)
        {
            isCritical = Random.Range(0f, 1f) <= critical;
            return isCritical ? atk * criticalAtk : atk;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}