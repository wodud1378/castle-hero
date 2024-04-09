using System;
using System.Collections.Generic;
using RGLabs.InGame.Behaviours.Unit.Components;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Utility;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace RGLabs.InGame.Behaviours.Unit
{
    public class UnitBehaviour : Obj
    {
        public event Action<UnitBehaviour> OnDead;
        
        public enum States
        {
            Idle,
            DefaultMove,
            MoveToTarget,
            Attack,
            Dead,
            Release,
        }

        private static readonly Dictionary<States, int> AnimationsHash = new()
        {
            { States.Idle, Animator.StringToHash("Idle") },
            { States.DefaultMove, Animator.StringToHash("Move") },
            { States.MoveToTarget, Animator.StringToHash("Move") },
            { States.Attack, Animator.StringToHash("Attack") },
            { States.Dead, Animator.StringToHash("Dead") },
        };

        [SerializeField] private int _randomSkinRange;
        [SerializeField] private SkeletonMecanim _skeletonMecanim;
        
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationEvents _animationEvents;

        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private int _maxAttackTarget;
        [SerializeField] private float _defaultMoveThreshlod;
        
        [SerializeField] private bool _canAttack;
        [SerializeField] private bool _canMove;
        
        [NonSerialized] public bool autoRelease = true;
        [NonSerialized] public Vector2 defaultDestination = default;

        public Vector2 Position
        {
            get => _rigidbody.position;
            set => _rigidbody.MovePosition(value);
        }
 
        private readonly RaycastHit2D[] _castBuffer = new RaycastHit2D[20];
        private readonly List<UnitBehaviour> _attackTargets = new();

        private UnitBehaviour _moveTarget;

        public States State
        {
            get => _state;
            private set
            {
                if (Equals(_state, value))
                    return;
                
                _state = value;
                UpdateAnimation(_state);

                if (_state == States.Idle)
                {
                    _moveTarget = null;
                    _attackTargets.Clear();
                }
            }
        }
        
        private States _state;
    
        private float _hp;
        private float _atk;
        private float _speed;
        private float _attackRange;
        private float _moveRange;

        private void Awake()
        {
            AttachAnimations();
        }

        public void Init(UnitEntity data)
        {
            ApplySkin(data.skinName);

            _hp = data.hp;
            _atk = data.atk;
            _speed = data.speed;
            _attackRange = data.attackRange;
            _moveRange = data.moveRange;

            _moveTarget = null;
            
            State = States.Idle;
            UpdateAnimation(State);
        }

        private void ApplySkin(string skinName)
        {
            if (string.IsNullOrEmpty(skinName))
                return;

            if (_skeletonMecanim == null)
                return;
            
            _skeletonMecanim.skeleton.SetSkin(skinName);
        }
        
        private void UpdateAnimation(States state)
        {
            if(_animator != null)
                _animator.SetTrigger(AnimationsHash[state]);
        }
        
        private void AttachAnimations()
        {
            if (_animator == null)
                return;
            
            _animationEvents.OnHitEvent -= ProcessHit;
            _animationEvents.OnHitEvent += ProcessHit;

            _animationEvents.OnReleaseAttackEvent -= OnReleaseAttack;
            _animationEvents.OnReleaseAttackEvent += OnReleaseAttack;
        }

        private int Search(float range)
        {
            int count = Physics2D.CircleCastNonAlloc(Position, range, default, _castBuffer, 0f, _enemyLayer);
            return count;
        }

        private bool SearchAttackTarget()
        {
            _attackTargets.RemoveAll((x) => !x.IsValid());
            
            int found = Search(_attackRange);
            if (found == 0)
                return false;

            int count = Mathf.Min(found, _maxAttackTarget);
            for (int i = 0; i < count; ++i)
            {
                if (!_castBuffer[i].collider.TryGetComponent(out UnitBehaviour unit))
                    continue;

                if (!unit.IsValid())
                    continue;

                _attackTargets.Add(unit);
            }

            return true;
        }

        private bool SearchMoveTarget()
        {
            int found = Search(_moveRange);
            if (found == 0)
                return false;

            UnitBehaviour firstFound = null;
            UnitBehaviour last = _moveTarget;
            UnitBehaviour duplicated = null;
            _moveTarget = null;
            
            for (int i = 0; i < found; ++i)
            {
                if (!_castBuffer[i].collider.TryGetComponent(out UnitBehaviour unit))
                    continue;

                if (!unit.IsValid())
                    continue;
                
                if (firstFound == null)
                    firstFound = unit;

                if (unit == last)
                    duplicated = unit;
            }

            _moveTarget = duplicated != null ? duplicated : firstFound;
            return true;
        }

        private void Update()
        {
            if (State is States.Release)
                return;

            UpdateState();
            ProcessState();
        }

        private void FixedUpdate()
        {
            if (State == States.MoveToTarget)
            {
                ProcessMoveToTarget();
                return;
            }

            if (State == States.DefaultMove)
            {
                ProcessDefaultMove();
                return;
            }
        }

        private void UpdateState()
        {
            if (_hp < 0)
            {
                State = States.Dead;
                return;
            }

            if (_canAttack)
            {
                if (CheckAttack())
                    return;   
            }

            if (_canMove)
            {
                if (CheckMoveToTarget())
                    return;

                if (CheckDefaultMove())
                    return;
            }

            State = States.Idle;
        }

        private void ProcessState()
        {
            switch (State)
            {
                case States.Idle:
                    ProcessIdle();
                    break;
                case States.Attack:
                    ProcessAttack();
                    break;
                case States.Dead:
                    ProcessDead();
                    break;
            }
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

        private bool CheckMoveToTarget()
        {
            if (State == States.MoveToTarget)
                return true;

            if (SearchMoveTarget())
            {
                State = States.MoveToTarget;
                return true;
            }

            return false;
        }

        private bool CheckDefaultMove()
        {
            if (Vector2.Distance(Position, defaultDestination) <= _defaultMoveThreshlod)
                return false;

            State = States.DefaultMove;
            return true;
        }

        private void ProcessIdle()
        {
            
        }
        
        private void ProcessDefaultMove()
        {
            Move(defaultDestination, _defaultMoveThreshlod);
        }

        private void ProcessMoveToTarget()
        {
            if (_moveTarget == null)
                return;

            Move(_moveTarget.Position, 0.32f);
        }

        private void Move(Vector2 target, float threshold)
        {
            var pos = Position;
            if (Vector2.Distance(pos, target) < threshold)
                return;

            var diff = target - pos;
            var dir = diff.normalized;
            var moveAmount = dir * (_speed * Time.fixedDeltaTime);
            
            Position += moveAmount;
            LookAt(target);
        }

        private void ProcessAttack()
        {
            if (_attackTargets.Count == 0)
                return;
            
            LookAt(_attackTargets[0].Position);
        }

        private void ProcessHit()
        {
            foreach (var target in _attackTargets)
            {
                target._hp -= _atk;
            }
        }

        private void ProcessDead()
        {
            if(autoRelease)
                DestroySelf();
            
            OnDead?.Invoke(this);
            OnDead = null;
        }

        private void OnReleaseAttack()
        {
            if (!CheckMoveToTarget())
                State = States.Idle;
        }

        private void LookAt(Vector2 target)
        {
            var diff = target - Position;
            var originScale = _animator.transform.localScale;
            float originX = Mathf.Abs(originScale.x);
            float scale = diff.x <= 0 ? originX : -originX;
            _animator.transform.localScale = new Vector3(scale, originScale.y, originScale.z);
        }

        private void OnDrawGizmosSelected()
        {
            if (_rigidbody == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Position, _moveRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Position, _attackRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Position, defaultDestination);
        }
    }
}