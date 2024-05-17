using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Finding
{
    public class FindingComponents
    {
        public readonly FindMoveTarget move;
        public readonly FindUnits attack;

        private readonly Status _status;
        
        public FindingComponents(Collider2D[] buffer, Status status)
        {
            move = new(buffer, 1);
            attack = new (buffer, 1);

            _status = status;
        }
        
        public void Init(LayerMask layerMask)
        {
            move.layerMask = layerMask;
            attack.layerMask = layerMask;
            
            Clear();
        }

        public bool TryFindMoveTarget(Vector2 position, out UnitBehaviour target)
        {
            move.range = _status.moveRange;
            
            target = null;
            if (!move.Update(position))
                return false;

            target = move.Found[0];
            return true;
        }

        public bool IsAbleToAttack(Vector2 position)
        {
            attack.range = _status.atkRange;
            
            return attack.Update(position);
        }

        public void Clear()
        {
            move.Clear();
            attack.Clear();
        }
    }
}