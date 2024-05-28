using RGLabs.InGame.System;

namespace RGLabs.Unit.Skill
{
    public class Skill10004 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            if (!TryGetQuantityParameter(0, out int quantity))
                return;

            if (!TryUpdateAroundCenter(quantity))
                return;
            
            float amount = GetAmount(type, value);
            PublishAtk(aroundCenter.center, DamageType.Normal, amount);
            foreach (var unit in aroundCenter.around)
            {
                PublishAtk(unit, DamageType.Normal, amount);
            }
            
            PlayEffect(0, aroundCenter.forward * Data.y);
        }
    }
}