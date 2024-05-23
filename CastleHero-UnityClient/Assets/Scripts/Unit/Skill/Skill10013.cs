using RGLabs.InGame.System;

namespace RGLabs.Unit.Skill
{
    public class Skill10013 : ActiveSkill
    {
        public enum Parameter
        {
            SingleAtk = 0,
            BoundAtk,
        }
        
        protected override void OnExecute()
        {
            if (!Targeting.HasTargets())
                return;

            var target = Targeting.Targets[0];
            if (TryGetStatusParameter(Parameter.SingleAtk, out var type, out var value))
                PublishAtk(target, DamageType.Normal, WithOwner(type, value));

            if (TryGetStatusParameter(Parameter.BoundAtk, out type, out value))
            {
                float amount = WithOwner(type, value);
                Bound.FindTargets(target.position, Data.range, default)
                    .ForEach(x=> PublishAtk(x, DamageType.Normal, amount));
            }
        }
    }
}