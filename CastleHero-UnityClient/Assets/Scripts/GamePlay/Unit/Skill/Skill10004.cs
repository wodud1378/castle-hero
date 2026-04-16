using CastleHero.GamePlay.Unit.Events;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Effects;

namespace CastleHero.GamePlay.Unit.Skill
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
            foreach (var unit in aroundCenter.units)
            {
                PublishAtk(unit, DamageType.Normal, amount);
            }
            
            if (TryGetEffectPrefab(0, out var effect))
            {
                Owner.EffectBuilder
                    .StartBuild(effect)
                    .To(aroundCenter.forward * Data.y)
                    .Run();
            }
        }
    }
}