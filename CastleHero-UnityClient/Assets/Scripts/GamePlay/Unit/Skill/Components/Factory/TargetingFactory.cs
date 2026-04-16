using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Finding;
using CastleHero.Utility;

namespace CastleHero.GamePlay.Unit.Skill.Components.Factory
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
                float range;
                Finder finder;
                switch (option)
                {
                    case Targeting.Alley:
                        finder = Finder.Create(IDetection.Option.Circle, maxTarget);
                        finder.detection.SetFilter(owner, Targeting.Alley);

                        range = owner.Status.atkRange;
                        finder.detection.SetRange(range, range);
                        break;
                    case Targeting.Enemy:
                        finder = owner.Core.Attack.finder;
                        break;
                    case Targeting.Both:
                        finder = Finder.Create(IDetection.Option.Circle, maxTarget);
                        finder.detection.SetFilter(owner, Targeting.Both);

                        range = owner.Status.atkRange;
                        finder.detection.SetRange(range, range);
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