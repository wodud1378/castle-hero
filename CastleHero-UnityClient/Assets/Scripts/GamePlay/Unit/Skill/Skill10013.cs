using CastleHero.GamePlay.Unit.Events;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Effects;

namespace CastleHero.GamePlay.Unit.Skill
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
                    Owner.EffectBuilder
                        .StartBuild(effect)
                        .To(center.Position)
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