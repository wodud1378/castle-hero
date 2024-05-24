using System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using RGLabs.Utility;

namespace RGLabs.Unit.Skill.Components.Factory
{
    public class BoundFactory
    {
        public IBound GetBound(IDetection.Option shape, Targeting targeting, UnitBehaviour owner, int maxTarget,
            float x, float y)
        {
            IBound bound;
            switch (shape)
            {
                case IDetection.Option.Circle:
                    bound = new CircleBound(x, y, maxTarget);
                    break;
                case IDetection.Option.Arc:
                    bound = new ArcBound(x, y, maxTarget);
                    break;
                case IDetection.Option.Box:
                    bound = new BoxBound(x, y, maxTarget);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
            }

            bound.Finder.detection.SetFilter(owner, targeting);
            return bound;
        }
    }
}