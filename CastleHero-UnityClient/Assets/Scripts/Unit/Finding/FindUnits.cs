using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Finding
{
    public class FindUnits
    {
        public enum Shape
        {
            Circle,
            Box
        }
        
        private static readonly Dictionary<Collider2D, UnitBehaviour> CachedUnits = new();

        public List<UnitBehaviour> Found { get; }

        public readonly IDetection detection;

        public FindUnits(IDetection detection)
        {
            this.detection = detection;

            Found = new();
        }

        protected virtual bool OnUpdate(int found)
        {
            int added = 0;
            for (int i = 0; i < found; ++i)
            {
                if (!TryGetUnit(detection.Buffer[i], out var unit))
                    continue;

                Found.Add(unit);
                ++added;
            }

            return added > 0;
        }

        public bool Update(Vector2 position)
        {
            Found.Clear();

            if (!detection.TrySearch(position, out int found))
                return false;

            return OnUpdate(found);
        }

        public void Clear() => Found.Clear();

        protected bool TryGetUnit(Collider2D collider, out UnitBehaviour unit)
        {
            if (!CachedUnits.TryGetValue(collider, out unit))
            {
                if (!collider.TryGetComponent(out unit))
                    return false;

                CachedUnits[collider] = unit;
            }

            return unit.IsValid();
        }

        public static FindUnits Create(Shape shape)
        {
            IDetection detection = null;
            var buffer = CreateBuffer();
            switch (shape)
            {
                case Shape.Circle:
                    detection = new CircleDetection { Buffer = buffer };
                    break;
                case Shape.Box:
                    detection = new BoxDetection { Buffer = buffer };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
            }
            
            return Create(detection);
        }

        public static FindUnits Create(IDetection detection)
        {
            return new(detection);
        }

        public static Collider2D[] CreateBuffer() => new Collider2D[Constants.BufferSize];
    }
}