using CastleHero.GamePlay.Unit.Events;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Finding;
using CastleHero.Utility;
using UnityEngine;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Effects;

namespace CastleHero.GamePlay.Unit.Components
{
    public class Attack
    {
        public readonly Finder finder;
        public readonly AdditionalAttack additional;

        private readonly UnitBehaviour _owner;
        private readonly IUnitRenderer _renderer;
        private readonly IAnimationEventProvider _animationEvents;

        public bool IsRunning { get; private set; }

        public UnitBehaviour CurrentTarget { get; private set; }

        public string projectile;

        public Attack(UnitBehaviour owner, Finder finder, IUnitRenderer renderer,
            IAnimationEventProvider animationEvents)
        {
            _owner = owner;

            this.finder = finder;
            additional = new(_owner);

            _renderer = renderer;
            _animationEvents = animationEvents;

            _animationEvents.OnHitEvent -= ProcessHit;
            _animationEvents.OnHitEvent += ProcessHit;

            _animationEvents.OnReleaseAttackEvent -= OnReleaseAttack;
            _animationEvents.OnReleaseAttackEvent += OnReleaseAttack;
        }

        public bool IsAbleToAttack()
        {
            var selected = Select();
            if (selected == null)
                return false;

            if (CurrentTarget != selected)
            {
                if (CurrentTarget.IsValid())
                    CurrentTarget.OnDead -= OnUnitDead;
                
                selected.OnDead -= OnUnitDead;
                selected.OnDead += OnUnitDead;
                CurrentTarget = selected;
            }

            return true;
        }
        
        private UnitBehaviour Select()
        {
            var overriden = finder.Override;
            if (overriden.IsValid())
                return overriden;

            float rangeStat = _owner.Status.atkRange;
            if (CurrentTarget.IsValid())
            {
                float distance = (CurrentTarget.Position - _owner.Position).sqrMagnitude;
                float range = Mathf.Pow(rangeStat, 2);

                if (distance <= range)
                    return CurrentTarget;
            }
            
            finder.detection.SetRange(rangeStat, rangeStat);
            return !finder.Update(_owner.Position)
                ? null
                : finder.Found.Count > 0
                    ? finder.Found[0]
                    : null;
        }
        
        private void OnUnitDead(UnitBehaviour unit)
        {
            if(unit == CurrentTarget)
                Stop();
            
            unit.OnDead -= OnUnitDead;
        }

        public void Run()
        {
            _renderer.SetAnimation(UnitCore.AnimationsHash[UnitCore.States.Attack]);
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

            if (!string.IsNullOrEmpty(projectile))_owner.EffectBuilder
                .StartBuild(projectile)
                .To(CurrentTarget)
                .From(_owner.Position)
                .Run();
        }

        private void OnReleaseAttack() => IsRunning = false;
    }
}