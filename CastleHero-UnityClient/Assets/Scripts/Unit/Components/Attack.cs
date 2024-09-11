using RGLabs.InGame.Effects.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Components
{
    public class Attack
    {
        public readonly Finder finder;
        public readonly AdditionalAttack additional;

        private readonly UnitBehaviour _owner;
        private readonly RenderController _renderController;
        private readonly AnimationEvents _animationEvents;

        public bool IsRunning { get; private set; }
        
        public UnitBehaviour CurrentTarget { get; private set; }
        
        public string projectile;        

        public Attack(UnitBehaviour owner, Finder finder, RenderController renderController,
            AnimationEvents animationEvents)
        {
            _owner = owner;

            this.finder = finder;
            additional = new(_owner);

            _renderController = renderController;
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

            float rangeStat = _owner.status.atkRange;
            if (CurrentTarget.IsValid())
            {
                float distance = (CurrentTarget.position - _owner.position).sqrMagnitude;
                float range = Mathf.Pow(rangeStat, 2);

                if (distance <= range)
                    return CurrentTarget;
            }
            
            finder.detection.SetRange(rangeStat, rangeStat);
            return !finder.Update(_owner.position)
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
            _renderController.SetAnimation(UnitCore.AnimationsHash[UnitCore.States.Attack]);
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
                Amount = _owner.status.atk
            }.Publish();
            
            additional.Execute(CurrentTarget);

            if (!string.IsNullOrEmpty(projectile))Effect.Builder
                .StartBuild(projectile)
                .To(CurrentTarget)
                .From(_owner.position)
                .Run();
        }

        private void OnReleaseAttack() => IsRunning = false;
    }
}