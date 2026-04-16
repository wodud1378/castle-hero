using System.Collections.Generic;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Finding;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Skill.Components
{
    public interface IBound
    {
        public Finder Finder { get; }
        public List<UnitBehaviour> UnitsInBound(Vector2 from, Vector2 forward);
    }

    public class CircleBound : IBound
    {
        public Finder Finder { get; }

        public CircleBound(float x, float y, int maxTarget = 0)
        {
            Finder = Finder.Create(IDetection.Option.Circle, maxTarget);
            Finder.detection.SetRange(x, y);
        }

        public List<UnitBehaviour> UnitsInBound(Vector2 from, Vector2 forward)
        {
            Finder.Update(from);

            return Finder.Found;
        }
    }

    public class ArcBound : IBound
    {
        public Finder Finder { get; }

        public ArcBound(float x, float y, int maxTarget = 0)
        {
            Finder = Finder.Create(IDetection.Option.Arc, maxTarget);
            Finder.detection.SetRange(x, x);
            Finder.detection.SetAngle(y);
        }

        public List<UnitBehaviour> UnitsInBound(Vector2 from, Vector2 forward)
        {
            Finder.detection.SetForward(forward);
            Finder.Update(from);

            return Finder.Found;
        }
    }
    
    public class BoxBound : IBound
    {
        public Finder Finder { get; }

        public BoxBound(float x, float y, int maxTarget = 0)
        {
            Finder = Finder.Create(IDetection.Option.Circle, maxTarget);
            Finder.detection.SetRange(x, y);
        }

        public List<UnitBehaviour> UnitsInBound(Vector2 from, Vector2 forward)
        {
            Finder.detection.SetForward(forward);
            Finder.Update(from);

            return Finder.Found;
        }
    }
}