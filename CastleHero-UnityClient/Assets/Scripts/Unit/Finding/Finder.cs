using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.Unit.Finding
{
    public class Finder
    {
        private static readonly DetectionFactory DetectionFactory = new();
        private static readonly Dictionary<Collider2D, UnitBehaviour> CachedUnits = new();

        public List<UnitBehaviour> Found { get; } = new();

        public UnitBehaviour Override => _overrides.Count > 0 ? _overrides[^1] : null;
        
        private readonly List<UnitBehaviour> _overrides = new();

        public readonly IDetection detection;

        private Finder(IDetection detection)
        {
            this.detection = detection;
        }

        private bool OnUpdate(int found)
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

        public void Clear()
        {
            Found.Clear();

            foreach (var unit in _overrides)
            {
                ReleaseOverride(unit);
            }

            _overrides.Clear();
        }

        public void RegisterOverride(UnitBehaviour unit)
        {
            _overrides.Add(unit);

            unit.OnDead += ReleaseOverride;
        }

        public void ReleaseOverride(UnitBehaviour unit)
        {
            if (_overrides.Contains(unit))
                _overrides.Remove(unit);
            
            unit.OnDead -= ReleaseOverride;
        }

        private bool TryGetUnit(Collider2D collider, out UnitBehaviour unit)
        {
            if (!CachedUnits.TryGetValue(collider, out unit))
            {
                if (!collider.TryGetComponent(out unit))
                    return false;

                CachedUnits[collider] = unit;
            }

            return unit.IsValid();
        }

        public static Finder Create(IDetection.Option option, int maxTarget, Collider2D[] buffer = null)
        {
            IDetection detection = DetectionFactory.GetDetection(option);

            buffer ??= new Collider2D[Constants.BufferSize];
            detection.Buffer = buffer;
            detection.MaxTarget = maxTarget == 0 ? Constants.BufferSize : maxTarget;

            var finder = new Finder(detection);
            return finder;
        }
    }
}