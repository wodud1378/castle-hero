using System;
using Cysharp.Threading.Tasks;
using CastleHero.Data;
using CastleHero.GamePlay.Unit.Events;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;

using CastleHero.Common;
namespace CastleHero.GamePlay.InGame.System
{
    public class UnitProcessor : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly IInGameSession _inGameRepository;
        private readonly EffectBuilder _effectBuilder;
        private readonly ISoundManager _soundManager;
        private readonly GameConstants _constants;

        public UnitProcessor(IInGameSession inGameRepository, EffectBuilder effectBuilder, ISoundManager soundManager, GameConstants constants)
        {
            _inGameRepository = inGameRepository;
            _effectBuilder = effectBuilder;
            _soundManager = soundManager;
            _constants = constants;

            SubscribeMessage<AtkEvent>(OnReceiveAtkEvent);
            SubscribeMessage<HealEvent>(OnReceiveHealEvent);
            SubscribeMessage<ShieldEvent>(ev => OnReceiveShieldEvent(ev).Forget());
            SubscribeMessage<RestrictionEvent>(OnReceiveRestrictionEvent);
            SubscribeMessage<StatusEffectEvent>(OnReceiveStatusEffectEvent);
            SubscribeMessage<UnitDead>(OnUnitDead);
            SubscribeMessage<GameFinished>(_ =>
            {
                var collection = _inGameRepository.Recovers;
                foreach (var recover in collection)
                {
                    recover.Clear();
                }
                
                collection.Dispose();
                collection.Clear();
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
                    critical = from.Status.critical;
                    criticalMul = from.Status.criticalAtk;
                    elementalMul = Elemental.AtkMultiplier(from.Core.Elemental, to.Core.Elemental);
                }

                damage = CalcAmount(ev.Amount, critical, criticalMul, elementalMul, out isCritical);
            }

            var status = to.Core.Status;
            status.shield.Decrease(damage, out float @protected, out float left);
            status.hp.Decrease(left);
            ev.Amount = left;

            if (to.Hit != null)
                to.Hit.Play();

            _soundManager.PlaySfx(to.Data.hitSfx);

            PlayEffect(ev)
                .Forget();

            new AtkResult { Event = ev, IsCritical = isCritical, Protected = @protected }.Publish();
        }

        private void OnReceiveHealEvent(HealEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            to.Status.hp.Increase(ev.Amount);

            PlayEffect(ev)
                .Forget();

            new HealResult { Event = ev }.Publish();
        }

        private async UniTaskVoid OnReceiveShieldEvent(ShieldEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            var shield = to.Status.shield;
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

            var ability = to.Status[ev.Type];
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

            float recoverTime = unit.Status.recovery;
            if (!unit.Core.EnableRecover || recoverTime <= 0f)
                return;

            var recover = new WaitRecover
            {
                behaviour = unit,
                position = unit.Core.Movement.Default,
                time = recoverTime
            };

            var subscription = ReserveRecover(recover);
            var collection = _inGameRepository.Recovers;
            collection.Add(recover);

            recover.Bind(collection, subscription);

            _effectBuilder
                .StartBuild(_constants.recoverEffect)
                .To(recover.behaviour.Position)
                .Duration(recover.time)
                .Run();
        }

        private async UniTask<IEffect> PlayEffect(IUnitEvent ev, float duration = 0f)
        {
            if (string.IsNullOrEmpty(ev.Effect))
                return null;

            var effect = await _effectBuilder
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
                .Subscribe(_ => Recovery(recover).Forget())
                .AddTo(_disposables);
        }

        private async UniTaskVoid Recovery(WaitRecover recover)
        {
            var behaviour = recover.behaviour;
            behaviour.Position = recover.position;

            _effectBuilder.Run(_constants.spawnEffect, behaviour.Position);

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