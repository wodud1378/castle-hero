using System;
using RGLabs.Common;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;

namespace RGLabs.Unit.Skill.Components
{
    public class BoundFactory
    {
        public enum Option
        {
            Circle,
            Arc,
            Box
        }

        public IBound GetBound(UnitBehaviour owner, Option option)
        {
            IBound bound;
            switch (option)
            {
                case Option.Circle:
                    bound = new CircleBound(owner);
                    break;
                case Option.Arc:
                    bound = new ArcBound(owner);
                    break;
                case Option.Box:
                    bound = new BoxBound(owner);
                    break;
                default:
                    bound = null;
                    break;
            }

            return bound;
        }
    }
    
    public class TargetFinderFactory
    {
        public enum Option
        {
            Self,
            Alley,
            Enemy,
            Both
        }
        
        public ITargeting GetTargeting(UnitBehaviour owner, Option option, int maxTarget)
        {
            if (maxTarget == 0)
                maxTarget = Constants.BufferSize;
            
            ITargeting targeting;
            if(option == Option.Self)
                targeting = new SelfTarget(owner);
            else
            {
                FindUnits finder;
                switch (option)
                {
                    case Option.Alley:
                        var detectAlley = new CircleDetection
                        {
                            Buffer = FindUnits.CreateBuffer(),
                            Mask = owner.Core.alleyLayerMask,
                            MaxTarget = maxTarget
                        };
                        
                        detectAlley.SetRange(owner.status.atkRange);
                        finder = FindUnits.Create(detectAlley);
                        break;
                    case Option.Enemy:
                        finder = owner.Core.finding.attack;
                        break;
                    case Option.Both:
                        var detectBoth = new CircleDetection
                        {
                            Buffer = FindUnits.CreateBuffer(),
                            Mask = owner.Core.alleyLayerMask | owner.Core.enemyLayerMask,
                            MaxTarget = maxTarget
                        };
                        detectBoth.SetRange(owner.status.atkRange);

                        finder = FindUnits.Create(detectBoth);
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