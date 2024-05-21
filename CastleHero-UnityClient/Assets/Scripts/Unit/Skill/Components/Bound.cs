using System.Collections.Generic;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using UnityEngine;

namespace RGLabs.Unit.Skill.Components
{
    public interface IBound
    {
        public IDetection Detection { get; }
        public List<UnitBehaviour> FindTargets(Vector2 forward);
    }

    public class CircleBound : IBound
    {
        protected readonly UnitBehaviour owner;
        protected readonly FindUnits finder;

        public Vector2 Forward { get; set; }
        public IDetection Detection => finder.detection;

        public CircleBound(UnitBehaviour owner)
        {
            this.owner = owner;
            
            finder = FindUnits.Create(FindUnits.Shape.Circle);
        }


        public virtual List<UnitBehaviour> FindTargets(Vector2 forward)
        {
            finder.Update(owner.position);

            return finder.Found;
        }
    }

    public class ArcBound : CircleBound
    {
        public float angle;
        
        private readonly Vector2[] _arcCheckBuffer;
        private readonly List<UnitBehaviour> _targets;

        public ArcBound(UnitBehaviour owner)
            : base(owner)
        {
            _arcCheckBuffer = new Vector2[4];
            _targets = new();
        }

        public override List<UnitBehaviour> FindTargets(Vector2 forward)
        {
            finder.Update(owner.position);

            _targets.Clear();
            var point = owner.position;
            foreach (var unit in finder.Found)
            {
                if (!InBound(point, forward, unit.Collider))
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

            float halfAngle = angle * 0.5f;
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
        private readonly UnitBehaviour _owner;
        private readonly FindUnits _finder;

        public IDetection Detection => _finder.detection;
   
        public BoxBound(UnitBehaviour owner)
        {
            _owner = owner;
            _finder = FindUnits.Create(FindUnits.Shape.Box);
        }

        public List<UnitBehaviour> FindTargets(Vector2 forward)
        {
            _finder.Update(_owner.position);

            return _finder.Found;
        }
    }
}