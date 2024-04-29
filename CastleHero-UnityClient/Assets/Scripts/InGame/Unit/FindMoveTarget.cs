using RGLabs.InGame.Behaviours.Unit;
using UnityEngine;

namespace RGLabs.InGame.Unit
{
    public class FindMoveTarget : FindUnits
    {
        public FindMoveTarget(Collider2D[] castBuffer, int maxTarget) 
            : base(castBuffer, maxTarget)
        {
        }
        
        protected override bool OnUpdate(int found)
        {
            if(Found.Count == 0)
                Found.Add(null);
            
            UnitBehaviour firstFound = null;
            UnitBehaviour last = Found[0];
            UnitBehaviour duplicated = null;
            
            Found[0] = null;

            for (int i = 0; i < found; ++i)
            {
                if (!TryGetUnit(_castBuffer[i], out var unit))
                    continue;
                
                if (firstFound == null)
                    firstFound = unit;

                if (unit == last)
                    duplicated = unit;
            }

            Found[0] = duplicated != null ? duplicated : firstFound;
            return Found[0] != null;
        }
    }
}