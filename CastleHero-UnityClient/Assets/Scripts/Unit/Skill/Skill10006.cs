using RGLabs.InGame.Effects.Behaviours;

namespace RGLabs.Unit.Skill
{
    public class Skill10006 : ActiveSkill
    {
        private enum Buff
        {
            Critical,
            AtkSpeed,
        }

    protected override void OnExecute()
        {
            if (!TryGetGroupParameter(0, out int group))
                return;

            if (!TryGetStatusParameter(Buff.Critical, out var type1, out var value1))
                return;
            
            if (!TryGetStatusParameter(Buff.AtkSpeed, out var type2, out var value2))
                return;

            if (!TryGetQuantityParameter(0, out int quantity))
                return;
            
            if (TryGetEffectPrefab(0, out var effect))
            {
                Effect.Builder
                    .StartBuild(effect)
                    .To(Targeting.Targets[0])
                    .Run();
            }
            
            var characters = Characters(x => x.Data.group == group, quantity);
            foreach (var character in characters)
            {
                PublishBuff(character, type1, value1, Data.duration, true);
                PublishBuff(character, type2, value2, Data.duration, true);
            }
        }
    }
}