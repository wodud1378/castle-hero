namespace RGLabs.Unit.Skill
{
    public class Skill10020 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetGroupParameter(0, out int group))
                return;

            if (!TryGetStatusParameter(0, out var type, out var value))
                return;
            
            if (!TryGetQuantityParameter(0, out int quantity))
                return;
            
            TryGetEffectPrefab(0, out string buffEff);
            
            var characters = Characters(x => x.Data.group == group, quantity);
            foreach (var character in characters)
            {
                PublishBuff(character, type, value, Data.duration, true, buffEff);
            }
        }
    }
}