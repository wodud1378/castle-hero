namespace CastleHero.GamePlay.Unit.Skill
{
    public class Skill20012 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity) ||
                !TryUpdateAroundCenter(quantity) ||
                !TryGetStatusParameter(0, out var type, out var value))
                return;

            float amount = GetAmount(type, value);
            foreach (var unit in aroundCenter.units)
            {
                TryGetEffectPrefab(0, out var effect);
                
                PublishHeal(unit, amount, effect);
            }
        }
    }
}