using RGLabs.InGame.Effects.Behaviours;
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
            
            if (TryGetEffectPrefab(Step.Atk, out var effect))
            {
                Effect.Builder
                    .StartBuild(effect)
                    .To( aroundCenter.center)
                    .Run();
            }
            
            foreach (var unit in aroundCenter.units)
            {
                totalDamage += amount;
                PublishAtk(unit, DamageType.Normal, amount);
            }

            if (!TryGetStatusParameter(Step.Heal, out type, out value))
                return;
            
            if (TryGetEffectPrefab(Step.Heal, out effect))
            {
                Effect.Builder
                    .StartBuild(effect)
                    .To(Owner)
                    .Run();
            }
            
            PublishHeal(Owner, totalDamage);
        }
    }
}