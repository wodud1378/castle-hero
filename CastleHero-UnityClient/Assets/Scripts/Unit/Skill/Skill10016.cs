using RGLabs.InGame.System;
using RGLabs.Utility;

namespace RGLabs.Unit.Skill
{
    public class Skill10016 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!Targeting.HasTargets())
                return;

            float totalDamage = 0f;
            if (TryGetStatusParameter(0, out var type, out var value))
            {
                var center = Targeting.Targets[0];
                var amount = WithOwner(type, value);
                Bound.FindTargets(center.position, Data.range, default)
                    .ForEach(x =>
                    {
                        totalDamage += amount;
                        PublishAtk(x, DamageType.Normal, amount);
                    });     
            }
            
            PublishHeal(Owner, totalDamage);
        }
    }
}