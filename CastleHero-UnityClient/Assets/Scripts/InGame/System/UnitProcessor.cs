using System;
using RGLabs.Common.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.System
{
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

    public enum DamageType
    {
        Normal,
        Debuff
    }

    public enum HealType
    {
        Heal,
        Shield
    }

    public interface IModifier
    {
        public UnitBehaviour From { get; }
        public UnitBehaviour To { get; }
        public float Amount { get; }
    }

    public struct AtkEvent : IModifier
    {
        public DamageType Type { get; set; }
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public float Amount { get; set; }
    }

    public struct HealEvent : IModifier
    {
        public HealType Type { get; set; }
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public float Amount { get; set; }
    }

    public struct AtkResult
    {
        public AtkEvent Event { get; set; }

        public bool IsCritical { get; set; }
    }

    public struct HealResult
    {
        public HealEvent Event { get; set; }
    }

    public class UnitProcessor
    {
        private readonly SceneBehaviour _root;

        public UnitProcessor(SceneBehaviour root)
        {
            _root = root;

            SubscribeMessage<AtkEvent>(OnReceiveAtkEvent);
            SubscribeMessage<HealEvent>(OnReceiveHealEvent);
            SubscribeMessage<WaitRecover>(OnCreatedRecover);
        }

        private void SubscribeMessage<T>(Action<T> onReceive)
        {
            MessageBroker.Default
                .Receive<T>()
                .Subscribe(onReceive)
                .AddTo(_root);
        }

        private void OnReceiveAtkEvent(AtkEvent ev)
        {
            var from = ev.From;
            var to = ev.To;
            if (!to.IsValid())
                return;

            float critical = 0f;
            float criticalMul = 0f;
            float elementalMul = 1f;
            if (from.IsValid())
            {
                critical = from.status.critical;
                criticalMul = from.status.criticalAtk;
                elementalMul = Elemental.AtkMultiplier(from.Core.elemental);
            }

            float amount = CalcAmount(ev.Amount, critical, criticalMul, elementalMul, out bool isCritical);

            to.Core.status.hp.Decrease(amount);

            if (to.Hit != null)
                to.Hit.Play();

            new AtkResult { Event = ev, IsCritical = isCritical }.Publish();
        }

        private void OnReceiveHealEvent(HealEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            to.status.hp.Increase(ev.Amount);

            new HealResult { Event = ev }.Publish();
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
            var stream = _root
                .UpdateAsObservable()
                .Select(_ => Time.deltaTime)
                .Where(x =>
                {
                    recover.leftTime -= x;
                    return recover.leftTime <= 0;
                });

            var subscription = stream
                .Subscribe(_ => Recovery(recover))
                .AddTo(_root);

            return subscription;
        }

        private void Recovery(WaitRecover recover)
        {
            recover.behaviour.Recovery(recover.position);
            recover.Dispose();
        }

        private float CalcAmount(float atk, float critical, float criticalAtk, float elementalAtk, out bool isCritical)
        {
            isCritical = Random.Range(0f, 1f) <= critical;
            return (isCritical ? atk * criticalAtk : atk) * elementalAtk;
        }
    }
}