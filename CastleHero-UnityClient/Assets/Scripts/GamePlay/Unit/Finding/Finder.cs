using System;
using System.Collections.Generic;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.Utility;
using UniRx;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Finding
{
    public class Finder
    {
        private static readonly DetectionFactory DetectionFactory = new();
        private static readonly Dictionary<Collider2D, UnitActor> CachedUnits = new();

        public List<UnitActor> Found { get; } = new();
        public UnitActor Main => Found.Count > 0 ? Found[0] : null;

        public UnitActor Override => _overrides.Count > 0 ? _overrides[^1] : null;
        
        public readonly IDetection detection;

        private readonly List<UnitActor> _overrides = new();
        private readonly Dictionary<UnitActor, IDisposable> _overrideDeadSubs = new();
        
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

        public void RegisterOverride(UnitActor unit)
        {
            _overrides.Add(unit);

            var sub = unit.OnDead.Take(1).Subscribe(ReleaseOverride);
            _overrideDeadSubs[unit] = sub;
        }

        public void ReleaseOverride(UnitActor unit)
        {
            if (_overrides.Contains(unit))
                _overrides.Remove(unit);

            if (_overrideDeadSubs.Remove(unit, out var sub))
                sub.Dispose();
        }

        private bool TryGetUnit(Collider2D collider, out UnitActor unit)
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