using System.Collections.Generic;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using UnityEngine;

namespace RGLabs.Unit.Skill.Components
{
    public interface IBound
    {
        public IDetection Detection { get; }
        public List<UnitBehaviour> FindTargets(Vector2 from, float range, Vector2 forward);
        public List<UnitBehaviour> FindTargets(Vector2 from, float x, float y, Vector2 forward);
    }

    public class CircleBound : IBound
    {
        private readonly FindUnits _finder = FindUnits.Create(FindUnits.Shape.Circle);

        public IDetection Detection => _finder.detection;

        public List<UnitBehaviour> FindTargets(Vector2 from, float range, Vector2 forward)
        {
            return FindTargets(from, range, range, forward);
        }

        public virtual List<UnitBehaviour> FindTargets(Vector2 from, float x, float y, Vector2 forward)
        {
            Detection.SetRange(x, y);

            return Find(from);
        }

        private List<UnitBehaviour> Find(Vector2 from)
        {
            _finder.Update(from);

            return _finder.Found;
        }
    }

    public class ArcBound : CircleBound
    {
        private const float Angle = 90f;

        private readonly Vector2[] _arcCheckBuffer = new Vector2[4];
        private readonly List<UnitBehaviour> _targets = new();

        public override List<UnitBehaviour> FindTargets(Vector2 from, float x, float y, Vector2 forward)
        {
            _targets.Clear();
            
            var targets = base.FindTargets(from, x, y, forward);
            foreach (var unit in targets)
            {
                if (!InBound(from, forward, unit.Collider))
                    continue;

                _targets.Add(unit);
            }

            return _targets;
        }

        private bool InBound(Vector2 point, Vector2 forward, Collider2D collider)
        {
            if (collider == null)
                return false;

            Vector2 targetPos = (Vector2)collider.transform.position - point;
            Vector2 halfSize = collider.bounds.size * 0.5f;

            // Left Up
            _arcCheckBuffer[0].Set(targetPos.x - halfSize.x, targetPos.y + halfSize.y);
            // Right Up
            _arcCheckBuffer[1].Set(targetPos.x + halfSize.x, targetPos.y + halfSize.y);
            // Left Bottom
            _arcCheckBuffer[2].Set(targetPos.x - halfSize.x, targetPos.y - halfSize.y);
            // Right Bottom
            _arcCheckBuffer[3].Set(targetPos.x + halfSize.x, targetPos.y - halfSize.y);

            float halfAngle = Angle * 0.5f;
            foreach (var buffer in _arcCheckBuffer)
            {
                float angle = Vector2.Angle(buffer, forward);
                if (angle <= halfAngle)
                    return true;
            }

            return false;
        }
    }

    public class BoxBound : IBound
    {
        private readonly FindUnits _finder = FindUnits.Create(FindUnits.Shape.Box);

        public IDetection Detection => _finder.detection;

        public List<UnitBehaviour> FindTargets(Vector2 from, float range, Vector2 forward)
        {
            Detection.SetRange(range);

            _finder.Update(from);
            return _finder.Found;
        }

        public List<UnitBehaviour> FindTargets(Vector2 from, float x, float y, Vector2 forward)
        {
            Detection.SetRange(x, y);

            _finder.Update(from);
            return _finder.Found;
        }
    }
}