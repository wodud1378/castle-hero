namespace RGLabs.Unit.Skill
{
    public class Skill10034 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            TryGetEffectPrefab(0, out string effect);

            var center = Targeting.Targets[0];
            Bound.UnitsInBound(center.position, default)
                .ForEach(x => PublishBuff(x, type, value, Data.duration, true, effect));
        }
    }
}