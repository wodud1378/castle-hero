using System;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;

namespace RGLabs.Unit.Skill
{
    public class Skill10002 : ActiveSkill
    {
        public enum Parameter
        {
            SingleAtk = 0,
            BoundAtk,
            DeBuff,
        }

        protected override void OnExecute()
        {
            if (!Targeting.HasTargets())
                return;

            var target = Targeting.Targets[0];
            if (TryGetStatusParameter(Parameter.SingleAtk, out var type, out var value))
            {
                PublishAtk(target, DamageType.Normal, WithOwner(type, value));
                PlayEffect(0, target);
            }

            var onEnemy = BuildExecutionOnEnemy();
            if (onEnemy == null)
                return;
            Bound.UnitsInBound(target.position, default)
                .ForEach(onEnemy);
        }

        private Action<UnitBehaviour> BuildExecutionOnEnemy()
        {
            Action<UnitBehaviour> action = null;
            if (TryGetStatusParameter(Parameter.BoundAtk, out var type, out var value))
            {
                float atkAmount = WithOwner(type, value);
                action += (x) => PublishAtk(x, DamageType.Normal, atkAmount);
            }

            if (TryGetStatusParameter(Parameter.DeBuff, out type, out value))
            {
                float debuffAmount = WithOwner(type, value);
                action += (x) =>
                {
                    x.status.speed.fixedAdjust.Decrease(debuffAmount);
                    x.status.atkSpeed.fixedAdjust.Decrease(debuffAmount);
                };
            }

            return action;
        }
    }
}