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

            var characters = Characters(x => x.Data.team == group);
            foreach (var character in characters)
            {
                character.status[type1].fixedAdjust.Increase(value1);
                character.status[type2].fixedAdjust.Increase(value2);
            }
        }
    }
}