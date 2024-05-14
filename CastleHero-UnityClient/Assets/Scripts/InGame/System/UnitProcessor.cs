using System.Linq;
using RGLabs.Common.Behaviours;
using RGLabs.Data.User;
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
    
    public struct WaitRecover
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
        private readonly SceneBehaviour _root;
        
        public UnitProcessor(SceneBehaviour root)
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
                .Receive<WaitRecover>()
                .Subscribe(OnReserveRecovery)
                .AddTo(_root);
            
            _root
                .UpdateAsObservable()
                .Select(_ => Time.deltaTime)
                .Subscribe(UpdateRecovery)
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

        private void OnReserveRecovery(WaitRecover recovery)
        {
            var array = _root.gameRepo.waitRecover.Value;
            _root.gameRepo.waitRecover.Value = array.Append(recovery).ToArray();
        }

        private void UpdateRecovery(float deltaTime)
        {
            var targets = _root.gameRepo.waitRecover.Value;
            for (int i = 0; i < targets.Length; ++i)
            {
                targets[i].time -= deltaTime;
                if (targets[i].time > 0f)
                    continue;
                
                Recovery(targets[i].behaviour, targets[i].position);
            }

            targets = targets.Where(x => x.time > 0f).ToArray();
            _root.gameRepo.waitRecover.Value = targets;
        }

        private void Recovery(UnitBehaviour behaviour, Vector2 position)
        {
            behaviour.ForceActivate();
            behaviour.Init(behaviour.Info, behaviour.Data);
            behaviour.position = position;
            behaviour.Core.defaultDestination = position;
        }

        private float CalcAmount(float atk, float critical, float criticalAtk, out bool isCritical)
        {
            isCritical = Random.Range(0f, 1f) <= critical;
            return isCritical ? atk * criticalAtk : atk;
        }

        private float CalcElemental(float amount, Elemental.Type atk, Elemental.Type def) => amount * Elemental.AtkMultiplier(atk, def);
    }
}