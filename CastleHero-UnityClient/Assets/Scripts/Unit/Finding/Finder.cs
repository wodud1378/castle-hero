using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Finding
{
    public class Finder
    {
        private static readonly DetectionFactory DetectionFactory = new();
        
        private static readonly Dictionary<Collider2D, UnitBehaviour> CachedUnits = new();

        public List<UnitBehaviour> Found { get; } = new();

        public IDetection detection;

        public Finder() { }
        
        public Finder(IDetection detection)
        {
            this.detection = detection;
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

        public static Finder Create(IDetection.Option option, int maxTarget, Collider2D[] buffer = null) => Create<Finder>(option, maxTarget, buffer);

        public static T Create<T>(IDetection.Option option, int maxTarget, Collider2D[] buffer = null) where T : Finder, new()
        {
            IDetection detection = DetectionFactory.GetDetection(option);
            
            buffer ??= new Collider2D[Constants.BufferSize];
            detection.Buffer = buffer;
            detection.MaxTarget = maxTarget == 0 ? Constants.BufferSize : maxTarget;

            var t = new T { detection = detection };
            return t;
        }
    }
}