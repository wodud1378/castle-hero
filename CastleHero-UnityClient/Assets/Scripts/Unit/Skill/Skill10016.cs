using RGLabs.InGame.System;

namespace RGLabs.Unit.Skill
{
    public class Skill10016 : ActiveSkill
    {
        private enum Step
        {
            Atk = 0,
            Heal,
        }
        
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity) ||
                !TryUpdateAroundCenter(quantity) ||
                !TryGetStatusParameter(Step.Atk, out var type, out var value))
                return;
            
            float totalDamage = 0f;
            float amount = GetAmount(type, value);
            PlayEffect(Step.Atk, aroundCenter.center);
            foreach (var unit in aroundCenter.units)
            {
                totalDamage += amount;
                PublishAtk(unit, DamageType.Normal, amount);
            }

            if (!TryGetStatusParameter(Step.Heal, out type, out value))
                return;
            
            PlayEffect(Step.Heal, Owner);
            PublishHeal(Owner, totalDamage);
        }
    }
}