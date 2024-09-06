using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.InGame.Effects;
using RGLabs.InGame.Effects.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.System
{
    public class UnitProcessor : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        public UnitProcessor()
        {
            SubscribeMessage<AtkEvent>(OnReceiveAtkEvent);
            SubscribeMessage<HealEvent>(OnReceiveHealEvent);
            SubscribeMessage<ShieldEvent>(OnReceiveShieldEvent);
            SubscribeMessage<RestrictionEvent>(OnReceiveRestrictionEvent);
            SubscribeMessage<StatusEffectEvent>(OnReceiveStatusEffectEvent);
            SubscribeMessage<UnitDead>(OnUnitDead);
            SubscribeMessage<GameFinished>(_ =>
            {
                foreach (var recover in Storage.inGameRepository.recovers)
                {
                    recover.Dispose();
                }
            });
        }

        public void Dispose() => _disposables.Dispose();

        private void SubscribeMessage<T>(Action<T> onReceive)
        {
            MessageBroker.Default
                .Receive<T>()
                .Subscribe(onReceive)
                .AddTo(_disposables);
        }

        private void OnReceiveAtkEvent(AtkEvent ev)
        {
            var from = ev.From;
            var to = ev.To;
            if (!to.IsValid())
                return;

            float damage;
            bool isCritical = false;
            if (to.Core.Invincible)
            {
                damage = 0f;
            }
            else
            {
                float critical = 0f;
                float criticalMul = 0f;
                float elementalMul = 1f;
                if (from.IsValid())
                {
                    critical = from.status.critical;
                    criticalMul = from.status.criticalAtk;
                    elementalMul = Elemental.AtkMultiplier(from.Core.elemental, to.Core.elemental);
                }

                damage = CalcAmount(ev.Amount, critical, criticalMul, elementalMul, out isCritical);
            }

            var status = to.Core.status;
            status.shield.Decrease(damage, out float @protected, out float left);
            status.hp.Decrease(left);
            ev.Amount = left;

            if (to.Hit != null)
                to.Hit.Play();

            Context.sounds.PlaySfx(to.Data.hitSfx);

            PlayEffect(ev)
                .Forget();

            new AtkResult { Event = ev, IsCritical = isCritical, Protected = @protected }.Publish();
        }

        private void OnReceiveHealEvent(HealEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            to.status.hp.Increase(ev.Amount);

            PlayEffect(ev)
                .Forget();

            new HealResult { Event = ev }.Publish();
        }

        private async void OnReceiveShieldEvent(ShieldEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            var shield = to.status.shield;
            var effect = await PlayEffect(ev, ev.Duration == 0f ? 999f : ev.Duration);

            if (ev.Duration == 0f)
                shield.Increase(ev.Amount, effect);
            else
                shield.Increase(ev.Amount, ev.Duration, effect);

            new HealResult { Event = ev }.Publish();
        }

        private void OnReceiveStatusEffectEvent(StatusEffectEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            var ability = to.status[ev.Type];
            var adjust = ev.IsMultiplier ? ability.multiplyAdjust : ability.fixedAdjust;
            if (ev.IsIncrease)
                adjust.Increase(ev.Amount);
            else
                adjust.Decrease(ev.Amount);

            PlayEffect(ev, ev.Duration)
                .Forget();
        }

        private void OnReceiveRestrictionEvent(RestrictionEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            to.Core.SetRestriction(ev.Type, ev.Duration);

            PlayEffect(ev, ev.Duration)
                .Forget();
        }

        private void OnUnitDead(UnitDead unitDead)
        {
            var unit = unitDead.unit;
            if (unit.Core.Team != UnitCore.Teams.Character || unit.Type == UnitBehaviour.BehaviourType.Barricade)
                return;

            Storage.inGameRepository.deadCharacters.Add(unit);

            float recoverTime = unit.status.recovery;
            if (!unit.Core.enableRecover || recoverTime <= 0f)
                return;

            var recover = new WaitRecover
            {
                behaviour = unit,
                position = unit.Core.movement.Default,
                time = recoverTime
            };

            var subscription = ReserveRecover(recover);
            var collection = Storage.inGameRepository.recovers;
            collection.Add(recover);

            recover.Bind(collection, subscription);

            Effect.Builder
                .StartBuild(Constants.RecoverEffect)
                .To(recover.behaviour.position)
                .Duration(recover.time)
                .Run();
        }

        private async UniTask<IEffect> PlayEffect(IUnitEvent ev, float duration = 0f)
        {
            if (string.IsNullOrEmpty(ev.Effect))
                return null;

            var effect = await Effect.Builder
                .StartBuild(ev.Effect)
                .To(ev.To)
                .Duration(duration)
                .RunAsync();

            return effect;
        }

        private IDisposable ReserveRecover(WaitRecover recover)
        {
            recover.leftTime.Value = recover.time;
            return Observable.EveryUpdate()
                .Select(_ => Time.deltaTime)
                .Select(x =>
                {
                    recover.leftTime.Value -= x;
                    return recover.leftTime.Value <= 0f;
                })
                .DistinctUntilChanged()
                .Where(isDone => isDone)
                .Take(1)
                .Subscribe(_ =>
                {
                    Storage.inGameRepository.deadCharacters.Remove(recover.behaviour);
                    Recovery(recover);
                })
                .AddTo(_disposables);
        }

        private async void Recovery(WaitRecover recover)
        {
            var behaviour = recover.behaviour;
            behaviour.position = recover.position;

            Effect.Builder.Run(Constants.SpawnEffect, behaviour.transform.position);

            for (int i = 0; i < 6; ++i)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            recover.behaviour.Recovery();
            recover.Dispose();
        }

        private float CalcAmount(float atk, float critical, float criticalAtk, float elementalAtk, out bool isCritical)
        {
            isCritical = Random.Range(0f, 1f) <= critical;
            return (isCritical ? atk * criticalAtk : atk) * elementalAtk;
        }
    }
}