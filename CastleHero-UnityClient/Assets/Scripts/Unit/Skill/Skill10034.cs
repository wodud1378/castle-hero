namespace RGLabs.Unit.Skill
{
    public class Skill10034 : ActiveSkill
    {
        protected override void OnExecute()
        {
            if (!TryGetStatusParameter(0, out var type, out var value))
                return;
            
            float increaseTime = Data.duration;
            float increaseValue = WithOwner(type, value); 
            var center = Targeting.Targets[0];
            Bound.FindTargets(center.position, Data.range, default)
                .ForEach(x =>
                { 
                    x.status.speed.fixedAdjust.Increase(increaseValue, increaseTime);
                });
        }
    }
}