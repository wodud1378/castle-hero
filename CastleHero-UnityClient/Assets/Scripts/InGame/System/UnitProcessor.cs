using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

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

    public class UnitProcessor
    {
        public UnitProcessor()
        {
            MessageBroker.Default.Receive<AtkEvent>().Subscribe(OnReceiveAtkEvent);
            MessageBroker.Default.Receive<HealEvent>().Subscribe(OnReceiveHealEvent);
        }

        private void OnReceiveAtkEvent(AtkEvent ev)
        {
            var to = ev.to;
            if (!to.IsValid())
                return;

            float amount = CalcAmount(ev.amount, ev.critical, ev.criticalMul);
            to.status.hp.Decrease(amount);
        }

        private void OnReceiveHealEvent(HealEvent ev)
        {
            var to = ev.to;
            if (!to.IsValid())
                return;

            to.status.hp.Increase(ev.amount);
        }

        private float CalcAmount(float atk, float critical, float criticalAtk)
        {
            bool isCritical = Random.Range(0f, 1f) <= critical;
            return isCritical ? atk * criticalAtk : atk;
        }
    }
}