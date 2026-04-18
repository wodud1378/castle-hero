using System;
using CastleHero.GamePlay.Unit.Events;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Finding;
using CastleHero.Utility;
using UniRx;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Components
{
    public class Attack
    {
        public readonly Finder finder;
        public readonly AdditionalAttack additional;

        private readonly UnitActor _owner;
        private readonly IUnitRenderer _renderer;
        private readonly IAnimationEventProvider _animationEvents;

        public bool IsRunning { get; private set; }

        public UnitActor CurrentTarget { get; private set; }

        public string projectile;

        private IDisposable _targetDeadSub;

        public Attack(UnitActor owner, Finder finder, IUnitRenderer renderer,
            IAnimationEventProvider animationEvents)
        {
            _owner = owner;

            this.finder = finder;
            additional = new(_owner);

            _renderer = renderer;
            _animationEvents = animationEvents;

            _animationEvents.OnHit.Subscribe(_ => ProcessHit()).AddTo(owner);
            _animationEvents.OnReleaseAttack.Subscribe(_ => OnReleaseAttack()).AddTo(owner);
        }

        public bool IsAbleToAttack()
        {
            var selected = Select();
            if (selected == null)
                return false;

            if (CurrentTarget != selected)
            {
                _targetDeadSub?.Dispose();
                _targetDeadSub = selected.OnDead.Take(1).Subscribe(OnUnitDead);
                CurrentTarget = selected;
            }

            return true;
        }
        
        private UnitActor Select()
        {
            return UnitHelper.SelectTarget(finder, _owner, CurrentTarget, _owner.Status.atkRange);
        }
        
        private void OnUnitDead(UnitActor unit)
        {
            if(unit == CurrentTarget)
                Stop();
        }

        public void Run()
        {
            _renderer.SetAnimation(CombatController.AnimationsHash[UnitState.States.Attack]);
            IsRunning = true;
        }

        public void Stop()
        {
            Clear();
        }

        public void Clear() => IsRunning = false;

        private void ProcessHit()
        {
            if (!CurrentTarget.IsValid())
                return;

            new AtkEvent
            {
                Type = DamageType.Normal,
                From = _owner,
                To = CurrentTarget,
                Amount = _owner.Status.atk
            }.Publish();

            additional.Execute(CurrentTarget);

            if (!string.IsNullOrEmpty(projectile))
            {
                new ProjectileEvent
                {
                    Prefab = projectile,
                    From = _owner,
                    To = CurrentTarget
                }.Publish();
            }
        }

        private void OnReleaseAttack() => IsRunning = false;
    }
}