using Cysharp.Threading.Tasks;
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

        
        public string projectile;
        
        private UnitBehaviour _target;

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
            float range = _owner.status.atkRange;
            var overrideUnit = finder.Override;
            if (overrideUnit.IsValid())
            {
                float distance = overrideUnit.position.DistanceTo(_owner.position);
                bool inRange = distance <= Mathf.Pow(range, 2f);
                _target = inRange ? overrideUnit : null;

                return inRange;
            }

            finder.detection.SetRange(range, range);

            if (!finder.Update(_owner.position))
                return false;

            _target = !_target.IsValid() 
                ? finder.Found[0] 
                : finder.Found.Contains(_target) 
                    ? _target
                    : finder.Found[0];
            
            return true;
        }

        public void Run()
        {
            _renderController.SetAnimation(UnitCore.AnimationsHash[UnitCore.States.Attack]);
            IsRunning = true;
        }

        public void Clear() => IsRunning = false;

        private void ProcessHit()
        {
            if (!_target.IsValid())
                return;

            new AtkEvent
            {
                Type = DamageType.Normal,
                From = _owner,
                To = _target,
                Amount = _owner.status.atk
            }.Publish();
            
            additional.Execute(_target);

            if (!string.IsNullOrEmpty(projectile))Effect.Builder
                .StartBuild(projectile)
                .To(_target)
                .From(_owner.position)
                .Run();
        }

        private void OnReleaseAttack() => IsRunning = false;
    }
}