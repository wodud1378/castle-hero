using UnityEngine;

namespace RGLabs.Unit.Finding
{
    public class FindingComponents
    {
        public readonly FindMoveTarget move;
        public readonly FindAttackTarget attack;

        public FindingComponents(Collider2D[] buffer, int maxAttackTarget)
        {
            move = new(buffer, 1);
            attack = new FindAttackTarget(buffer, maxAttackTarget);
        }

        public void Init(LayerMask layerMask)
        {
            move.layerMask = layerMask;
            attack.layerMask = layerMask;
            
            Clear();
        }

        public void Clear()
        {
            move.Clear();
            attack.Clear();
        }
    }
}