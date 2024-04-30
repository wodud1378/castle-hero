using System;
using System.Collections.Generic;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Utility;
using UnityEngine;

namespace RGLabs.InGame.Unit
{
    [Serializable]
    public abstract class FindUnits
    {
        private static readonly Dictionary<Collider2D, UnitBehaviour> CachedUnits = new();
        
        public List<UnitBehaviour> Found { get; }

        [SerializeField] public LayerMask layerMask;
        
        protected readonly Collider2D[] _castBuffer;
        
        protected readonly int _maxTarget;
        
        public FindUnits(Collider2D[] castBuffer, int maxTarget)
        {
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
        
        protected virtual bool TrySearch(Vector2 position, float range, out int found)
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