using RGLabs.InGame.System;

namespace RGLabs.Unit.Skill
{
    public class Skill10003 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!Targeting.HasTargets())
                return;
            
            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            var target = Targeting.Targets[0]; 
            PublishAtk(target, DamageType.Normal, WithOwner(type, value));
            PlayEffect(0, target);
        }
    }
}