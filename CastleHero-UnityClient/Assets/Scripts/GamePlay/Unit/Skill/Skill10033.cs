using CastleHero.GamePlay.Unit.Events;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Effects;

namespace CastleHero.GamePlay.Unit.Skill
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
            
            if (TryGetEffectPrefab(0, out var effect))
            {
                Owner.EffectBuilder
                    .StartBuild(effect)
                    .To( aroundCenter.center)
                    .Run();
            }
            
            foreach (var unit in aroundCenter.units)
            {
                PublishAtk(unit, DamageType.Normal, amount);
            }

            if (!TryGetGroupParameter(1, out int group))
                return;

            if (!TryGetStatusParameter(1, out type, out value))
                return;

            TryGetEffectPrefab(1, out effect);
            amount = GetAmount(type, value);
            float duration = Data.duration;
            var characters = Characters(x => x.Data.group == group);
            foreach (var unit in characters)
            {
                unit.Core.Attack.additional.Add(Owner, amount, effect, duration);
            }
        }
    }
}