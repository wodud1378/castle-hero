using RGLabs.InGame.System;

namespace RGLabs.Unit.Skill
{
    public class Skill10033 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity) ||
                !TryUpdateAroundCenter(quantity) ||
                !TryGetStatusParameter(0, out var type, out var value))
                return;
            
            float amount = GetAmount(type, value);
            PlayEffect(0, aroundCenter.center);
            foreach (var unit in aroundCenter.units)
            {
                PublishAtk(unit, DamageType.Normal, amount);
            }
        }
    }
}