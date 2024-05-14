using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.System
{
    public enum DamageType
    {
        Normal,
        DeBuff,
    }
    
    public struct ReserveRecovery
    {
        public UnitBehaviour behaviour;
        public Vector2 position;
        public float time;
    }
    
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
        public DamageType type;
        public UnitBehaviour from;
        public UnitBehaviour to;
        public bool isCritical;
        public float amount;
    }

    public class UnitProcessor
    {
        private readonly MonoBehaviour _root;
        
        public UnitProcessor(MonoBehaviour root)
        {
            _root = root;
            
            MessageBroker.Default
                .Receive<AtkEvent>()
                .Subscribe(OnReceiveAtkEvent)
                .AddTo(_root);

            MessageBroker.Default
                .Receive<HealEvent>()
                .Subscribe(OnReceiveHealEvent)
                .AddTo(_root);

            MessageBroker.Default
                .Receive<ReserveRecovery>()
                .Subscribe(OnReserveRecovery)
                .AddTo(_root);
        }

        private void OnReceiveAtkEvent(AtkEvent ev)
        {
            var from = ev.from;
            var to = ev.to;
            if (!to.IsValid())
                return;

            float amount;
            amount = CalcAmount(ev.amount, ev.critical, ev.criticalMul, out bool isCritical);

            if (from.IsValid())
                amount = CalcElemental(amount, from.Core.elemental.atkType, to.Core.elemental.defType);
                    
            to.Core.status.hp.Decrease(amount);
            
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

        private void OnReserveRecovery(ReserveRecovery recovery)
        {
            float currentTime = recovery.time;
            _root
                .UpdateAsObservable()
                .Select(_ => Time.deltaTime)
                .Where(x =>
                {
                    currentTime -= x;
                    return currentTime <= 0f;
                })
                // TODO 유닛 부활 로직
                .Subscribe(_ =>
                {
                    var behaviour = recovery.behaviour;
                    behaviour.Activate();
                    behaviour.Init(behaviour.Data);
                    behaviour.position = recovery.position;
                })
                .AddTo(_root);
        }

        private float CalcAmount(float atk, float critical, float criticalAtk, out bool isCritical)
        {
            isCritical = Random.Range(0f, 1f) <= critical;
            return isCritical ? atk * criticalAtk : atk;
        }

        private float CalcElemental(float amount, Elemental.Type atk, Elemental.Type def) => amount * Elemental.AtkMultiplier(atk, def);
    }
}