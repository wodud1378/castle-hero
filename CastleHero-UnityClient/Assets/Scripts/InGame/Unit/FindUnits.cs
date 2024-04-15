using System.Collections.Generic;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Utility;
using UnityEngine;

namespace RGLabs.InGame.Unit
{
    public abstract class FindUnits
    {
        private static readonly Dictionary<Collider2D, UnitBehaviour> CachedUnits = new();
        
        public List<UnitBehaviour> Found { get; }

        protected readonly LayerMask _layerMask;
        protected readonly RaycastHit2D[] _castBuffer;
        protected readonly int _maxTarget;

        public FindUnits(LayerMask layerMask, RaycastHit2D[] castBuffer, int maxTarget)
        {
            _layerMask = layerMask;
            _castBuffer = castBuffer;
            _maxTarget = maxTarget;
            
            Found = new();
        }

        protected abstract bool OnUpdate(int found);

        public bool Update(Vector2 position, float range)
        {
            if (!TrySearch(position, range, out int found))
                return false;
            
            return OnUpdate(found);
        }
        
        public void Clear() => Found.Clear();
        
        protected bool TrySearch(Vector2 position, float range, out int found)
        {
            found = Physics2D.CircleCastNonAlloc(position, range, default, _castBuffer, 0f, _layerMask);
            if (_maxTarget > 0)
                found = Mathf.Min(found, _maxTarget);

            return found > 0;
        }
        
        protected bool TryGetUnit(RaycastHit2D hit, out UnitBehaviour unit)
        {
            var collider = hit.collider;
            if (!CachedUnits.TryGetValue(collider, out unit))
            {
                if (!collider.TryGetComponent(out unit))
                    return false;

                CachedUnits[collider] = unit;
            }

            if (unit.IsValid())
                return true;

            unit = null;
            return false;
        }
    }
}