using RGLabs.InGame.Effects.Behaviours;
using RGLabs.Unit.Components;

namespace RGLabs.Unit.Skill
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
                PublishRestriction(unit, UnitCore.Restrictions.Attack, duration);
                PublishRestriction(unit, UnitCore.Restrictions.Skill, duration);
                PublishRestriction(unit, UnitCore.Restrictions.Move, duration);
            } 
            
            if (TryGetEffectPrefab(0, out var effect))
            {
                Effect.Builder
                    .StartBuild(effect)
                    .To(Owner)
                    .LookAt(aroundCenter.forward)
                    .Run();
            }
        }
    }
}