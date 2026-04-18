using System;
using CastleHero.Data;
using CastleHero.GamePlay.Unit.Events;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.GamePlay.Unit;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;
using CastleHero.Common;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.Utility;
namespace CastleHero.GamePlay.InGame.System
{
    public class UnitProcessor : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly IInGameSession _inGameSession;
        private readonly EffectBuilder _effectBuilder;
        private readonly ISoundManager _soundManager;
        private readonly GameConstants _constants;

        public UnitProcessor(IInGameSession inGameSession, EffectBuilder effectBuilder, ISoundManager soundManager, GameConstants constants)
        {
            _inGameSession = inGameSession;
            _effectBuilder = effectBuilder;
            _soundManager = soundManager;
            _constants = constants;

            SubscribeMessage<AtkEvent>(OnReceiveAtkEvent);
            SubscribeMessage<HealEvent>(OnReceiveHealEvent);
            SubscribeMessage<ShieldEvent>(OnReceiveShieldEvent);
            SubscribeMessage<RestrictionEvent>(OnReceiveRestrictionEvent);
            SubscribeMessage<StatusEffectEvent>(OnReceiveStatusEffectEvent);
            SubscribeMessage<ProjectileEvent>(OnReceiveProjectileEvent);
            SubscribeMessage<UnitDead>(OnUnitDead);
            SubscribeMessage<GameFinished>(_ =>
            {
                var collection = _inGameSession.Recovers;
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
            var to = ev.To;
            if (!to.IsValid())
                return;

            float damage = CalcDamage(ev, out bool isCritical);
            float @protected = ApplyDamage(to, damage, ev);

            PlayHitFeedback(to, ev);

            new AtkResult { Event = ev, IsCritical = isCritical, Protected = @protected }.Publish();
        }

        private float CalcDamage(AtkEvent ev, out bool isCritical)
        {
            isCritical = false;
            if (ev.To.Combat.Invincible)
                return 0f;

            float critical = 0f;
            float criticalMul = 0f;
            float elementalMul = 1f;
            if (ev.From.IsValid())
            {
                critical = ev.From.Status.critical;
                criticalMul = ev.From.Status.criticalAtk;
                elementalMul = Elemental.AtkMultiplier(ev.From.UnitState.Elemental, ev.To.UnitState.Elemental);
            }

            return CalcAmount(ev.Amount, critical, criticalMul, elementalMul, out isCritical);
        }

        private float ApplyDamage(UnitActor to, float damage, AtkEvent ev)
        {
            var status = to.Combat.Status;
            status.shield.Decrease(damage, out float @protected, out float left);
            status.hp.Decrease(left);
            ev.Amount = left;
            return @protected;
        }

        private void PlayHitFeedback(UnitActor to, AtkEvent ev)
        {
            if (to.Hit != null)
                to.Hit.Play();

            _soundManager.PlaySfx(to.Data.hitSfx);

            PlayEffect(ev);
        }

        private void OnReceiveHealEvent(HealEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            to.Status.hp.Increase(ev.Amount);

            PlayEffect(ev);

            new HealResult { Event = ev }.Publish();
        }

        private void OnReceiveShieldEvent(ShieldEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            var shield = to.Status.shield;
            var effect = PlayEffect(ev, ev.Duration == 0f ? 999f : ev.Duration);

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

            PlayEffect(ev, ev.Duration);
        }

        private void OnReceiveRestrictionEvent(RestrictionEvent ev)
        {
            var to = ev.To;
            if (!to.IsValid())
                return;

            to.Combat.SetRestriction(ev.Type, ev.Duration);

            PlayEffect(ev, ev.Duration);
        }

        private void OnReceiveProjectileEvent(ProjectileEvent ev)
        {
            if (!ev.To.IsValid())
                return;

            _effectBuilder
                .StartBuild(ev.Prefab)
                .To(ev.To)
                .From(ev.From.Position)
                .Run();
        }

        private void OnUnitDead(UnitDead unitDead)
        {
            var unit = unitDead.unit;

            if (unit.UnitState.Team == UnitState.Teams.Monster)
            {
                _effectBuilder.Run(_constants.deadEffect, unit.Position);
                _effectBuilder.Run(_constants.manaDropEffect, unit.Position);

                // int 랜덤은 맥스값 - 1, 가독성을 위해 +1.
                _inGameSession.Mana.Value += Random.Range(3, 10 + 1);
            }

            if (unit.UnitState.Team != UnitState.Teams.Character || unit.Type == UnitActor.ActorType.Barricade)
                return;

            float recoverTime = unit.Status.recovery;
            if (!unit.UnitState.EnableRecover || recoverTime <= 0f)
                return;

            var recover = new WaitRecover
            {
                actor = unit,
                position = unit.Combat.Movement.Default,
                time = recoverTime
            };

            var subscription = ReserveRecover(recover);
            var collection = _inGameSession.Recovers;
            collection.Add(recover);

            recover.Bind(collection, subscription);

            _effectBuilder
                .StartBuild(_constants.recoverEffect)
                .To(recover.actor.Position)
                .Duration(recover.time)
                .Run();
        }

        private IEffect PlayEffect(IUnitEvent ev, float duration = 0f)
        {
            if (string.IsNullOrEmpty(ev.Effect))
                return null;

            return _effectBuilder
                .StartBuild(ev.Effect)
                .To(ev.To)
                .Duration(duration)
                .Run();
        }

        private const int RecoveryDelayFrames = 6;

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
                .Subscribe(_ => Recovery(recover))
                .AddTo(_disposables);
        }

        private void Recovery(WaitRecover recover)
        {
            var actor = recover.actor;
            actor.Position = recover.position;

            _effectBuilder.Run(_constants.spawnEffect, actor.Position);

            Observable.TimerFrame(RecoveryDelayFrames)
                .Subscribe(_ =>
                {
                    recover.actor.Recovery();
                    recover.Dispose();
                })
                .AddTo(_disposables);
        }

        private float CalcAmount(float atk, float critical, float criticalAtk, float elementalAtk, out bool isCritical)
        {
            isCritical = Random.Range(0f, 1f) <= critical;
            return (isCritical ? atk * criticalAtk : atk) * elementalAtk;
        }
    }
}