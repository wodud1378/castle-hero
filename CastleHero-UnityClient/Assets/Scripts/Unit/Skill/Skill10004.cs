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

            PublishAtk(Targeting.Targets[0], DamageType.Normal, WithOwner(type, value));
        }
    }
}