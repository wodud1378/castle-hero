using System;
using RGLabs.Common.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.System
{
    public class UnitProcessor
    {
        private readonly SceneBehaviour _root;

        public UnitProcessor(SceneBehaviour root)
        {
            _root = root;

            SubscribeMessage<AtkEvent>(OnReceiveAtkEvent);
            SubscribeMessage<HealEvent>(OnReceiveHealEvent);
            SubscribeMessage<ShieldEvent>(OnReceiveShieldEvent);
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

            float damage;
            bool isCritical = false;
            if (!to.Core.Invincible)
            {
                float critical = 0f;
                float criticalMul = 0f;
                float elementalMul = 1f;
                if (from.IsValid())
                {
                    critical = from.status.critical;
                    criticalMul = from.status.criticalAtk;
                    elementalMul = Elemental.AtkMultiplier(from.Core.elemental);
                }
                damage = CalcAmount(ev.Amount, critical, criticalMul, elementalMul, out isCritical);
            }
            else
            {
                damage = 0f;
            }

            var status = to.Core.status;
            status.shield.Decrease(damage, out float @protected, out float left);
            status.hp.Decrease(left);
            ev.Amount = left;

            if (to.Hit != null)
                to.Hit.Play();

            new AtkResult { Event = ev, IsCritical = isCritical, Protected = @protected}.Publish();
        }

        private void OnReceiveHealEvent(HealEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            to.status.hp.Increase(ev.Amount);

            new HealResult { Event = ev }.Publish();
        }

        private void OnReceiveShieldEvent(ShieldEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            var shield = to.status.shield;
            if (ev.Duration == 0f)
                shield.Increase(ev.Amount);
            else
                shield.Increase(ev.Amount, ev.Duration);

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