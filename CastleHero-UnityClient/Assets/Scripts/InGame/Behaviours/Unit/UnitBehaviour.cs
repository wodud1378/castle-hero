using System;
using System.Collections.Generic;
using RGLabs.InGame.Behaviours.Unit.Components;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Unit;
using RGLabs.InGame.Utility;
using Spine.Unity;
using UnityEngine;

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
            //{ States.Dead, Animator.StringToHash("Dead") },
        };

        [SerializeField] private SkeletonMecanim _skeletonMecanim;

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;
        
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

        [field:SerializeField] public Status Status { get; private set; }

        public States State
        {
            get => _state;
            private set
            {
                if (Equals(_state, value))
                    return;

                _state = value;
                switch (_state)
                {
                    case States.Idle:
                        _findMoveTarget.Clear();
                        _findAttackTarget.Clear();
                        UpdateAnimation(_state);
                        break;
                    case States.Dead:
                        break;
                    default:
                        UpdateAnimation(_state);
                        break;
                }
            }
        }

        private readonly RaycastHit2D[] _castBuffer = new RaycastHit2D[20];

        private States _state;

        private FindUnits _findMoveTarget;
        private FindUnits _findAttackTarget;
        private RenderController _renderController;
        private Attack _attack;

        public void Init(UnitEntity data)
        {
            State = States.Idle;
            Status.Init(data);

            _findMoveTarget.Clear();
            _findAttackTarget.Clear();
            _renderController.ApplySkin(data.skinName);
            UpdateAnimation(State);
        }

        public Vector2 ClosestPoint(UnitBehaviour other) => ClosestPoint(Position, other);

        public Vector2 ClosestPoint(Vector2 position, UnitBehaviour other)
        {
            var closest = other._collider.ClosestPoint(position);
            var ranged = (closest - position).normalized * (Status.atkRange * 0.9f);
            var final = closest - ranged;
            return final;
        }

        private void Awake()
        {
            Status = new();

            _findMoveTarget = new FindMoveTarget(_enemyLayer, _castBuffer, 0);
            _findAttackTarget = new FindAttackTarget(_enemyLayer, _castBuffer, _maxAttackTarget);
            _renderController = new RenderController(_skeletonMecanim, _animator);
            _attack = new Attack();

            AttachAnimationEvents();
        }

        private void UpdateAnimation(States state) => _renderController.SetAnimation(AnimationsHash[state]);

        private void AttachAnimationEvents()
        {
            if (_animator == null)
                return;

            _animationEvents.OnHitEvent -= ProcessHit;
            _animationEvents.OnHitEvent += ProcessHit;

            _animationEvents.OnReleaseAttackEvent -= OnReleaseAttack;
            _animationEvents.OnReleaseAttackEvent += OnReleaseAttack;
        }

        private void Update()
        {
            if (State is States.Release)
                return;

            Status.Update();

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
            if (Status.hp <= 0)
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

            if (_findAttackTarget.Update(Position, Status.atkRange))
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

            if (_findMoveTarget.Update(Position, Status.moveRange))
            {
                State = States.MoveToTarget;
                return true;
            }

            return false;
        }

        private bool CheckDefaultMove()
        {
            if (Position.IsNear(defaultDestination, 0.1f))
                return false;

            State = States.DefaultMove;
            return true;
        }

        private void ProcessDefaultMove()
        {
            Move(defaultDestination, 0);
        }

        private void ProcessMoveToTarget()
        {
            var target = _findMoveTarget.Found[0];
            if (!target.IsValid())
                return;
            
            Move(ClosestPoint(target), 0f);
        }

        private void Move(Vector2 target, float threshold)
        {
            var pos = Position;
            if (pos.IsNear(target, threshold))
                return;

            var diff = target - pos;
            var dir = diff.normalized;
            var moveAmount = dir * (Status.speed * Time.fixedDeltaTime);

            Position += moveAmount;
            LookAt(target);
        }

        private void ProcessAttack()
        {
            var targets = _findAttackTarget.Found;
            if (targets.Count == 0)
                return;

            LookAt(targets[0].Position);
        }

        private void ProcessHit() => _attack.Process(this, _findAttackTarget.Found);

        private void ProcessDead()
        {
            if (autoRelease)
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
            if (_rigidbody == null || Status == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Position, Status.moveRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Position, Status.atkRange);

            if (State == States.DefaultMove)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(Position, defaultDestination);
            }
        }
    }
}