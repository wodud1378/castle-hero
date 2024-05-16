using System;
using System.Collections.Generic;
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
    
    public class WaitRecover : IDisposable
    {
        public UnitBehaviour behaviour;
        public Vector2 position;
        public float leftTime;

        private IDisposable _subscription;
        private ReactiveCollection<WaitRecover> _root;

        public void Bind(ReactiveCollection<WaitRecover> root, IDisposable subscription)
        {
            _root = root;
            _subscription = subscription;
        }
        
        public void Dispose()
        {
            _root?.Remove(this);
            _subscription?.Dispose();
        }
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
                .Subscribe(OnCreatedRecover)
                .AddTo(_root);
        }

        private void OnReceiveAtkEvent(AtkEvent ev)
        {
            var from = ev.from;
            var to = ev.to;
            if (!to.IsValid())
                return;

            float amount = CalcAmount(ev.amount, ev.critical, ev.criticalMul, out bool isCritical);
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

        private void OnCreatedRecover(WaitRecover recover)
        {
            var subscription = ReserveRecover(recover);
            var collection = _root.gameRepo.recovers;
            collection.Add(recover);
            
            recover.Bind(collection, subscription);
        }

        private IDisposable ReserveRecover(WaitRecover recover)
        {
            float deltaTime = recover.leftTime;
            var stream = _root
                .UpdateAsObservable()
                .Select(_ => Time.deltaTime)
                .Where(x =>
                {
                    recover.leftTime -= deltaTime;
                    return recover.leftTime <= 0;
                });
            
            var subscription = stream
                .Subscribe(_=> Recovery(recover))
                .AddTo(_root);

            return subscription;
        }
        
        private void Recovery(WaitRecover recover)
        {
            var behaviour = recover.behaviour;
            var position = recover.position;
            
            behaviour.ForceActivate();
            behaviour.Init(behaviour.Info, behaviour.Data);
            behaviour.position = position;
            behaviour.Core.defaultDestination = position;
            
            recover.Dispose();
        }

        private float CalcAmount(float atk, float critical, float criticalAtk, out bool isCritical)
        {
            isCritical = Random.Range(0f, 1f) <= critical;
            return isCritical ? atk * criticalAtk : atk;
        }

        private float CalcElemental(float amount, Elemental.Type atk, Elemental.Type def) => amount * Elemental.AtkMultiplier(atk, def);
    }
}