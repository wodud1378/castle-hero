namespace RGLabs.Unit.Skill
{
    public class Skill10006 : ActiveSkill
    {
        public enum Parameter
        {
            Critical,
            AtkSpeed,
        }

    protected override void OnExecute()
        {
            if (!TryGetGroupParameter(0, out int group))
                return;

            if (!TryGetStatusParameter(Parameter.Critical, out var type1, out var value1))
                return;
            
            if (!TryGetStatusParameter(Parameter.AtkSpeed, out var type2, out var value2))
                return;

            TryGetEffectPrefab(0, out string effect);
            
            var characters = Characters(x => x.Data.team == group);
            foreach (var character in characters)
            {
                PublishBuff(character, type1, value1, Data.duration, true, effect);
                PublishBuff(character, type2, value2, Data.duration, true);
            }
        }
    }
}