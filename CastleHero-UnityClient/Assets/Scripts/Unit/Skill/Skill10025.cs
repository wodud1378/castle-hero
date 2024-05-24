namespace RGLabs.Unit.Skill
{
    public class Skill10025 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            float amount = WithOwner(type, value);
            Bound.UnitsInBound(Owner.position, default)
                .ForEach(x => PublishShield(x, amount, 0f));
        }
    }
}