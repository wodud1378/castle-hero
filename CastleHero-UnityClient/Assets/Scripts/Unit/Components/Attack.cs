using RGLabs.InGame.Effects.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using RGLabs.Utility;

namespace RGLabs.Unit.Components
{

    public class Attack
    {
        public readonly Finder finder;

        private readonly UnitBehaviour _unit;
        private readonly AnimationEvents _animationEvents;
        
        public bool IsRunning { get; private set; }

        public string projectile;

        public Attack(UnitBehaviour unit, Finder finder)
        {
            _unit = unit;
            
            this.finder = finder;

            _animationEvents = unit.Core.animationEvent;

            _animationEvents.OnHitEvent -= ProcessHit;
            _animationEvents.OnHitEvent += ProcessHit;

            _animationEvents.OnReleaseAttackEvent -= OnReleaseAttack;
            _animationEvents.OnReleaseAttackEvent += OnReleaseAttack;
        }

        public bool IsAbleToAttack()
        {
            float range = _unit.status.atkRange;
            finder.detection.SetRange(range, range);
            
            if (!finder.Update(_unit.position))
                return false;

            return true;
        }
        
        public void Run() => IsRunning = true;

        private void ProcessHit()
        {
            var targets = finder.Found;
            foreach (var target in targets)
            {
                if (!target.IsValid())
                    continue;

                var data = new AtkEvent
                {
                    Type = DamageType.Normal,
                    From = _unit,
                    To = target,
                    Amount = _unit.status.atk
                };

                data.Publish();
                
                if(!string.IsNullOrEmpty(projectile))
                    Effect.Play(projectile, target);
            }
        }

        private void OnReleaseAttack() => IsRunning = false;
    }
}