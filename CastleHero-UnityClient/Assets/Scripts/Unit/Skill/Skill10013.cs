using RGLabs.InGame.Effects.Behaviours;
using RGLabs.InGame.System;

namespace RGLabs.Unit.Skill
{
    public class Skill10013 : ActiveSkill
    {
        private enum Step
        {
            SingleAtk = 0,
            BoundAtk,
        }
        
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity))
                return;

            if (!TryUpdateAroundCenter(quantity))
                return;

            if (!TryGetStatusParameter(Step.SingleAtk, out var type, out var value))
            {
                var center = aroundCenter.center;
                if (TryGetEffectPrefab(0, out var effect))
                {
                    Effect.Builder
                        .StartBuild(effect)
                        .To(center.position)
                        .Run();
                }
                PublishAtk(center, DamageType.Normal, GetAmount(type, value));    
            }

            if (TryGetStatusParameter(Step.BoundAtk, out type, out value))
            {
                float amount = GetAmount(type, value);
                foreach (var unit in aroundCenter.units)
                {
                    PublishAtk(unit, DamageType.Normal, amount);
                }
            }
        }
    }
}