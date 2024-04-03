using UnityEngine;

namespace RGLabs.InGame.Behaviours.Unit
{
    public class GameUnit : Obj
    {
        public enum States
        {
            Idle,
            Move,
            Attack,
            Dead,
        }

        public States State { get; protected set; } = States.Idle;

        [SerializeField] private float _hp;
        [SerializeField] private float _speed;
        
        [SerializeField] private float _atk;
        [SerializeField] private float _atkSpeed;
        [SerializeField] private float _atkRange;

        [SerializeField] private float _enemySearchRange;
        [SerializeField] private float _attractionEnemyRange;
        [SerializeField] private int _attractionPeriod;
        
        private GameUnit _attractionTarget = null;
        
        private readonly RaycastHit2D[] _searchBuffer = new RaycastHit2D[20];

        private void Attract()
        {
            if (_attractionTarget == null)
                return;
            
            
        }

        private void Attack()
        {
            
        }
        
        private void SearchEnemy()
        {
            int found = Physics2D.CircleCastNonAlloc(transform.position, _enemySearchRange, Vector2.zero, _searchBuffer);
            if (found <= 0)
                return;

            int index = -1;
            int max = -int.MaxValue;
            for (int i = 0; i < found; ++i)
            {
                var unit = _searchBuffer[i].collider.GetComponent<GameUnit>();
                if (max < unit._attractionPeriod)
                {
                    index = i;
                    max = unit._attractionPeriod;
                }
            }

            if (index is -1)
                return;

            var candidate = _searchBuffer[index].collider.GetComponent<GameUnit>();
            if (_attractionTarget is null)
                _attractionTarget = candidate;
            else
            {
                _attractionTarget = _attractionTarget._attractionPeriod < candidate._attractionPeriod
                    ? candidate
                    : _attractionTarget;
            }
        }
    }
}