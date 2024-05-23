using RGLabs.Common;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using RGLabs.Utility;

namespace RGLabs.Unit.Skill.Components.Factory
{
    public enum Targeting
    {
        Self,
        Alley,
        Enemy,
        Both
    }
    
    public class TargetingFactory
    {
        public ITargeting GetTargeting(Targeting option, UnitBehaviour owner, int maxTarget)
        {
            if (maxTarget == 0)
                maxTarget = Constants.BufferSize;
            
            ITargeting targeting;
            if(option == Targeting.Self)
                targeting = new SelfTarget(owner);
            else
            {
                Finder finder;
                switch (option)
                {
                    case Targeting.Alley:
                        finder = Finder.Create(IDetection.Option.Circle, maxTarget);
                        finder.detection.SetFilter(owner, Targeting.Alley);
                        finder.detection.SetRange(owner.status.atkRange);
                        break;
                    case Targeting.Enemy:
                        finder = owner.Core.attack.finder;
                        break;
                    case Targeting.Both:
                        finder = Finder.Create(IDetection.Option.Circle, maxTarget);
                        finder.detection.SetFilter(owner, Targeting.Both);
                        finder.detection.SetRange(owner.status.atkRange);
                        break;
                    default:
                        return null;
                }

                targeting = new FindTargets(owner, finder);
            }

            return targeting;
        }
    }
}