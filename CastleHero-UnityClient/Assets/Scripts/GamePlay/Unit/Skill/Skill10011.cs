using CastleHero.GamePlay.Unit.Components;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Effects;

namespace CastleHero.GamePlay.Unit.Skill
{
    public class Skill10011 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity) ||
                !TryUpdateAroundCenter(quantity))
                return;
            
            float duration = Data.duration;
            foreach (var unit in aroundCenter.units)
            {
                PublishRestriction(unit, CombatController.Restrictions.Attack, duration);
                PublishRestriction(unit, CombatController.Restrictions.Skill, duration);
                PublishRestriction(unit, CombatController.Restrictions.Move, duration);
            } 
            
            if (TryGetEffectPrefab(0, out var effect))
            {
                Owner.EffectBuilder
                    .StartBuild(effect)
                    .To(Owner)
                    .LookAt(aroundCenter.forward)
                    .Run();
            }
        }
    }
}