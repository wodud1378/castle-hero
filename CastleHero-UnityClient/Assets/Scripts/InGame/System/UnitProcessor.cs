using RGLabs.Common;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Utility;
using UnityEngine;

namespace RGLabs.InGame.System
{
    public struct AdjustHpEvent
    {
        public UnitBehaviour from;
        public UnitBehaviour to;

        public float amount;
        public float critical;
        public float criticalMul;
    }

    public class UnitStreamHandler
    {
        public UnitStreamHandler(DataStream<AdjustHpEvent> atkStream, DataStream<AdjustHpEvent> healStream)
        {
        }

        private void OnCollectAtkData(AdjustHpEvent ev)
        {
            var to = ev.to;
            if (!to.IsValid())
                return;

            float amount = CalcAmount(ev.amount, ev.critical, ev.criticalMul);
            to.Status.hp.Decrease(amount);
        }

        private void OnCollectHealData(AdjustHpEvent ev)
        {
            var to = ev.to;
            if (!to.IsValid())
                return;

            to.Status.hp.Increase(ev.amount);
        }

        private float CalcAmount(float atk, float critical, float criticalAtk)
        {
            bool isCritical = Random.Range(0f, 1f) <= critical;
            return isCritical ? atk * criticalAtk : atk;
        }
    }
}