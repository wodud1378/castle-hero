using System;
using System.Collections.Generic;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Finding
{
    public class FindUnits
    {
        private static readonly Dictionary<Collider2D, UnitBehaviour> CachedUnits = new();
        
        public List<UnitBehaviour> Found { get; }
        public LayerMask layerMask;
        public float range;
        
        protected readonly Collider2D[] _castBuffer;

        private readonly int _maxTarget;

        public FindUnits(Collider2D[] castBuffer, int maxTarget)
        {
            _castBuffer = castBuffer;
            _maxTarget = maxTarget;

            Found = new();
        }

        protected virtual bool OnUpdate(int found)
        {
            int added = 0;
            for (int i = 0; i < found; ++i)
            {
                if (!TryGetUnit(_castBuffer[i], out var unit))
                    continue;

                Found.Add(unit);
                ++added;
            }

            return added > 0;
        }

        public bool Update(Vector2 position)
        {
            Found.Clear();
            
            if (!TrySearch(position, out int found))
                return false;
            
            return OnUpdate(found);
        }
        
        public void Clear() => Found.Clear();
        
        protected virtual bool TrySearch(Vector2 position, out int found)
        {
            found =  Physics2D.OverlapCircleNonAlloc(position, range, _castBuffer, layerMask);
            if (_maxTarget > 0)
                found = Mathf.Min(found, _maxTarget);

            return found > 0;
        }
        
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
    }
}