using CastleHero.GamePlay.Unit.Events;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Effects;

namespace CastleHero.GamePlay.Unit.Skill
{
    public class Skill10007 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            if (!TryGetQuantityParameter(0, out int quantity))
                return;

            if (!TryUpdateAroundCenter(quantity, true))
                return;
            
            var center = aroundCenter.center;
            
            if (TryGetEffectPrefab(0, out var effect))
            {
                Owner.EffectBuilder
                    .StartBuild(effect)
                    .To(center.Position)
                    .Run();
            }

            float amount = GetAmount(type, value);
            foreach (var unit in aroundCenter.units)
            {
                PublishAtk(unit, DamageType.Normal, amount);
            }

            Bound.UnitsInBound(center.Position, default)
                .ForEach(x => PublishAtk(x, DamageType.Normal, GetAmount(type, value)));
        }
    }
}