using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using RGLabs.Utility;

namespace RGLabs.Unit.Components
{
    public class Attack
    {
        private readonly UnitBehaviour _unit;
        private readonly FindAttackTarget _finder;
        private readonly AnimationEvents _animationEvents;
        
        public bool InProgress { get; private set; }

        public ProjectileLauncher projectileLauncher;

        public Attack(UnitBehaviour unit, FindAttackTarget finder, AnimationEvents animationEvents)
        {
            _unit = unit;
            _finder = finder;
            _animationEvents = animationEvents;

            _animationEvents.OnHitEvent -= ProcessHit;
            _animationEvents.OnHitEvent += ProcessHit;

            _animationEvents.OnReleaseAttackEvent -= OnReleaseAttack;
            _animationEvents.OnReleaseAttackEvent += OnReleaseAttack;
        }

        public void Execute() => InProgress = true;

        private void ProcessHit()
        {
            var targets = _finder.Found;
            foreach (var target in targets)
            {
                if (!target.IsValid())
                    continue;

                var status = _unit.status;
                var data = new AtkEvent
                {
                    from = _unit,
                    to = target,
                    amount = status.atk,
                    critical = status.critical,
                    criticalMul = status.criticalAtk
                };

                data.Publish();
            }

            projectileLauncher?.Launch(targets);
        }

        private void OnReleaseAttack() => InProgress = false;
    }
}