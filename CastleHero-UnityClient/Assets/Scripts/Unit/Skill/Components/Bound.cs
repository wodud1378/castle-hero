using System.Collections.Generic;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using UnityEngine;

namespace RGLabs.Unit.Skill.Components
{
    public interface IBound
    {
        public Finder Finder { get; }
        public List<UnitBehaviour> UnitsInBound(Vector2 from, Vector2 forward);
    }

    public class CircleBound : IBound
    {
        public Finder Finder { get; }

        public CircleBound(float range, int maxTarget = 0)
        {
            Finder = Finder.Create(IDetection.Option.Circle, maxTarget);
            Finder.detection.SetRange(range);
        }


        public List<UnitBehaviour> UnitsInBound(Vector2 from, Vector2 forward)
        {
            Finder.Update(from);

            return Finder.Found;
        }
    }

    public class ArcBound : IBound
    {
        private const float Angle = 90f;
        public Finder Finder { get; }

        public ArcBound(float range, int maxTarget = 0)
        {
            Finder = Finder.Create(IDetection.Option.Arc, maxTarget);
            Finder.detection.SetRange(range);
            Finder.detection.Angle = Angle;
        }


        public List<UnitBehaviour> UnitsInBound(Vector2 from, Vector2 forward)
        {
            Finder.Update(from);

            return Finder.Found;
        }
    }
    
    public class BoxBound : IBound
    {
        public Finder Finder { get; }

        public IDetection Detection => Finder.detection;

        public BoxBound(float x, float y, int maxTarget = 0)
        {
            Finder = Finder.Create(IDetection.Option.Circle, maxTarget);
        }


        public List<UnitBehaviour> UnitsInBound(Vector2 from, Vector2 forward)
        {
            Finder.Update(from);

            return Finder.Found;
        }
    }
}