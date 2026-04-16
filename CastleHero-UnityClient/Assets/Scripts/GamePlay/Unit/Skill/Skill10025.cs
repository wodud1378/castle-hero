using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Effects;
namespace CastleHero.GamePlay.Unit.Skill
{
    public class Skill10025 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity) ||
                !TryUpdateAroundCenter(quantity) ||
                !TryGetStatusParameter(0, out var type, out var value))
                return;

            TryGetEffectPrefab(1, out string eff);
            
            float amount = GetAmount(type, value);
            foreach (var unit in aroundCenter.units)
            {
                if (TryGetEffectPrefab(0, out var effect))
                {
                    Owner.EffectBuilder
                        .StartBuild(effect)
                        .To(unit)
                        .Run();
                }
                
                PublishShield(unit, amount, 0f, eff);
            }
        }
    }
}