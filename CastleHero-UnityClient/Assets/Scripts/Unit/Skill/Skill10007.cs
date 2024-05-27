using RGLabs.InGame.System;

namespace RGLabs.Unit.Skill
{
    public class Skill10007 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            if (!TryGetQuantityParameter(0, out int quantity))
                return;

            if (!TryUpdateAroundCenter(quantity, true))
                return;
            
            var center = aroundCenter.center;
            PlayEffect(0, center.position);

            float amount = GetAmount(type, value);
            foreach (var unit in aroundCenter.around)
            {
                PublishAtk(unit, DamageType.Normal, amount);
            }
            
            Bound.UnitsInBound(center.position, default)
                .ForEach(x => PublishAtk(x, DamageType.Normal, GetAmount(type, value)));
        }
    }
}