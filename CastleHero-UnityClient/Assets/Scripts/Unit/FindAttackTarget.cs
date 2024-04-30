using System;
using UnityEngine;

namespace RGLabs.Unit
{
    [Serializable]
    public class FindAttackTarget : FindUnits
    {
        public FindAttackTarget(Collider2D[] castBuffer, int maxTarget) 
            : base(castBuffer, maxTarget)
        {
        }
        
        protected override bool OnUpdate(int found)
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
    }
}