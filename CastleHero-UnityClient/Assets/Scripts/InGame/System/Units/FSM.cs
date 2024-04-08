using System.Collections.Generic;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Behaviours.Unit.Components;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.System.Units
{
    public class FSM
    {
        public enum States
        {
            None,
            Idle,
            Move,
            Attack,
            Dead
        }

        public readonly List<UnitBehaviour> Attackable;
        
        public States State { get; private set; }
        public ReactiveProperty<float> Hp { get; } 
     
        private readonly RaycastHit2D[] _castBuffer;
        private readonly LayerMask _enemyLayer;
        
        private UnitBehaviour _moveTarget;
        private readonly float _moveDetectRange;
        private readonly float _attackDetectRange;
        private readonly int _maxAttackTarget;
        
        private Vector2 _position;
        
        public FSM(LayerMask enemyLayer, float hp, float moveDetectRange, float attackDetectRange, int maxAttackTarget)
        {
            _enemyLayer = enemyLayer;
            _castBuffer = new RaycastHit2D[20];
            _moveDetectRange = moveDetectRange;
            _attackDetectRange = attackDetectRange;
            _maxAttackTarget = maxAttackTarget;
            
            Attackable = new();
            Hp = new(hp);
        }
        
        public States UpdateState(Vector2 position)
        {
            _position = position;
            
            if (Hp.Value < 0)
            {
                State = States.Dead;
                return State;
            }

            if (CheckAttack())
                return State;

            if (CheckMove())
                return State;

            State = States.Idle;
            return State;
        }
        
        private bool CheckAttack()
        {
            if (State == States.Attack)
                return true;

            if (SearchAttackTarget())
            {
                State = States.Attack;
                return true;
            }

            return false;
        }

        private bool CheckMove()
        {
            if (State == States.Move)
                return true;

            if (SearchMoveTarget())
            {
                State = States.Attack;
                return true;
            }

            return false;
        }
        
        private int Search(float range)
        {
            int count = Physics2D.CircleCastNonAlloc(_position, range, default, _castBuffer, 0f, _enemyLayer);
            return count;
        }

        private bool SearchAttackTarget()
        {
            int found = Search(_attackDetectRange);
            if (found == 0)
                return false;

            int count = Mathf.Min(found, _maxAttackTarget);
            for (int i = 0; i < count; ++i)
            {
                if (!_castBuffer[i].collider.TryGetComponent(out UnitBehaviour unit))
                    continue;

                Attackable.Add(unit);
            }

            return true;
        }

        private bool SearchMoveTarget()
        {
            int found = Search(_moveDetectRange);
            if (found == 0)
                return false;

            UnitBehaviour firstFound = null;
            UnitBehaviour duplicated = null;
            for (int i = 0; i < found; ++i)
            {
                if (!_castBuffer[i].collider.TryGetComponent(out UnitBehaviour unit))
                    continue;

                if (firstFound == null)
                    firstFound = unit;

                if (unit == _moveTarget)
                    duplicated = unit;
            }

            _moveTarget = duplicated == null ? firstFound : duplicated;
            return true;
        }
    }
}