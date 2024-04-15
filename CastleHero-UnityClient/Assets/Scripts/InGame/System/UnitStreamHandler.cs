using RGLabs.Common;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Utility;
using UnityEngine;

namespace RGLabs.InGame.System
{
    public struct AdjustHpRequest
    {
        public UnitBehaviour from;
        public UnitBehaviour to;

        public float amount;
        public float critical;
        public float criticalMul;
    }

    public class UnitStreamHandler
    {
        private readonly DataStream<AdjustHpRequest> _atkStream;
        private readonly DataStream<AdjustHpRequest> _healStream;

        public UnitStreamHandler(DataStream<AdjustHpRequest> atkStream, DataStream<AdjustHpRequest> healStream)
        {
            _atkStream = atkStream;
            _healStream = healStream;

            _atkStream.Collect += OnCollectAtkData;
            _healStream.Collect += OnCollectHealData;
        }

        private void OnCollectAtkData(AdjustHpRequest request)
        {
            var to = request.to;
            if (!to.IsValid())
                return;

            float amount = CalcAmount(request.amount, request.critical, request.criticalMul);
            to.Status.hp.Decrease(amount);
        }

        private void OnCollectHealData(AdjustHpRequest request)
        {
            var to = request.to;
            if (!to.IsValid())
                return;

            to.Status.hp.Increase(request.amount);
        }

        private float CalcAmount(float atk, float critical, float criticalAtk)
        {
            bool isCritical = Random.Range(0f, 1f) <= critical;
            return isCritical ? atk * criticalAtk : atk;
        }
    }
}