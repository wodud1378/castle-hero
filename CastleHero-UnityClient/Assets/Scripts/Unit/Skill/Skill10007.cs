using RGLabs.InGame.System;

namespace RGLabs.Unit.Skill
{
    public class Skill10007 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!Targeting.HasTargets())
                return;

            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            var center = Targeting.Targets[0];
            Bound.UnitsInBound(center.position, default)
                .ForEach(x => PublishAtk(x, DamageType.Normal, WithOwner(type, value)));
        }
    }
}