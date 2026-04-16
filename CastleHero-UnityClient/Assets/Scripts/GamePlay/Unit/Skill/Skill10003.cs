using CastleHero.GamePlay.Unit.Events;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Effects;

namespace CastleHero.GamePlay.Unit.Skill
{
    public class Skill10003 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!Targeting.HasTargets())
                return;
            
            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            var target = Targeting.Targets[0]; 
            PublishAtk(target, DamageType.Normal, GetAmount(type, value));
            if (TryGetEffectPrefab(0, out var effect))
            {
                Owner.EffectBuilder
                    .StartBuild(effect)
                    .To(target)
                    .Run();
            }
        }
    }
}