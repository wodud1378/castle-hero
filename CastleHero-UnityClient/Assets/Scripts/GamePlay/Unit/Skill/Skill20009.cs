using System.Linq;

namespace CastleHero.GamePlay.Unit.Skill
{
    public class Skill20009 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity) ||
                !TryUpdateAroundCenter(quantity) ||
                !TryGetStatusParameter(0, out var type, out var value))
                return;

            TryGetEffectPrefab(0, out string eff);
            
            float amount = GetAmount(type, value);
            var target = aroundCenter.units
                .OrderBy(x => x.Status.hp.Left)
                .FirstOrDefault();
            
            PublishHeal(target, amount, eff);
        }
    }
}