using System;
using RGLabs.Unit.Behaviours;

namespace RGLabs.Unit.Finding
{
    public class FindMoveTarget : Finder
    {
        public FindMoveTarget() : base() {}
        
        public FindMoveTarget(IDetection detection) : base(detection)
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
                if (!TryGetUnit(detection.Buffer[i], out var unit))
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