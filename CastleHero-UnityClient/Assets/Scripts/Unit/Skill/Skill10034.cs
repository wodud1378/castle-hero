namespace RGLabs.Unit.Skill
{
    public class Skill10034 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity) ||
                !TryUpdateAroundCenter(quantity, true) ||
                !TryGetStatusParameter(0, out var type, out var value))
                return;

            TryGetEffectPrefab(0, out string effect);
            foreach (var unit in aroundCenter.around)
            {
                PublishBuff(unit, type, value, Data.duration, true, effect);
            }
        }
    }
}