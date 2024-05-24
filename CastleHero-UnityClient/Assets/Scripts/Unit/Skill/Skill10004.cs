using RGLabs.InGame.System;

namespace RGLabs.Unit.Skill
{
    public class Skill10004 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!Targeting.HasTargets())
                return;

            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            var center = Targeting.Targets[0].position;
            float amount = WithOwner(type, value);
            Bound.Finder.detection.SetForward((center - Owner.position).normalized);
            Bound.UnitsInBound(center, default)
                .ForEach(x => { PublishAtk(x, DamageType.Normal, amount ); });

            PublishAtk(Targeting.Targets[0], DamageType.Normal, WithOwner(type, value));
        }
    }
}