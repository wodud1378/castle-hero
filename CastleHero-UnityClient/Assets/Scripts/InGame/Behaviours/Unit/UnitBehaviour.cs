using System;
using System.Collections.Generic;
using RGLabs.InGame.Behaviours.Unit.Components;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Unit;
using RGLabs.InGame.Utility;
using Spine.Unity;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RGLabs.InGame.Behaviours.Unit
{
    public class UnitBehaviour : Obj
    {
        public event Action<UnitBehaviour> OnDead;

        public enum States
        {
            Prepare,
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

        private const int LookFrameThreshold = 10;

        [Header("Renderer")] [SerializeField] private SkeletonMecanim _skeletonMecanim;
        [SerializeField] private MeshRenderer _meshRenderer;

        [Header("Physics")] [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;

        [Header("Animation")] [SerializeField] private Animator _animator;
        [SerializeField] private AnimationEvents _animationEvents;

        [Header("Others")] [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private int _maxAttackTarget;
        [SerializeField] private float _defaultMoveThreshlod;
        [SerializeField] private bool _canAttack;
        [SerializeField] private bool _canMove;
        [SerializeField] private Vector2 _offset;

        [NonSerialized] public bool autoRelease = true;
        [NonSerialized] public Vector2 defaultDestination = default;
        
        public Vector2 Position
        {
            get => _rigidbody.position;
            set => _rigidbody.MovePosition(value);
        }

        public Vector2 Center => Position + _offset;

        public Status Status { get; private set; }

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

        private Vector2 _look;
        private int _currentLookFrame;

        public void Init(UnitEntity data)
        {
            Status.Init(data);

            _findMoveTarget.Clear();
            _findAttackTarget.Clear();
            _renderController.ApplySkin(data.skinName);

            _currentLookFrame = LookFrameThreshold;
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
            State = States.Prepare;
            Status = new();

            _findMoveTarget = new FindMoveTarget(_enemyLayer, _castBuffer, 0);
            _findAttackTarget = new FindAttackTarget(_enemyLayer, _castBuffer, _maxAttackTarget);
            _renderController = new RenderController(_skeletonMecanim, _animator);

            _attack = new Attack();

            AttachAnimationEvents();
        }

        private void UpdateAnimation(States state)
        {
            if (!AnimationsHash.TryGetValue(state, out var hash))
                return;
            
            _renderController.SetAnimation(hash);
        }

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
            UpdateLookDirection();
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
            if (State == States.Prepare)
            {
                State = States.Idle;
                return;
            }
            
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

        private void UpdateLookDirection()
        {
            ++_currentLookFrame;

            if (LookFrameThreshold > _currentLookFrame)
                return;

            LookAt(_look);
            _currentLookFrame = 0;
        }

        private bool CheckAttack()
        {
            if (State == States.Attack)
                return true;

            if (_findAttackTarget.Update(Center, Status.atkRange))
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

            if (_findMoveTarget.Update(Center, Status.moveRange))
            {
                if (Position.IsNear(_findMoveTarget.Found[0].Position, 0f))
                    return false;
                
                State = States.MoveToTarget;
                return true;
            }

            return false;
        }

        private bool CheckDefaultMove()
        {
            if (Position.IsNear(defaultDestination, _defaultMoveThreshlod))
                return false;

            State = States.DefaultMove;
            return true;
        }

        private void ProcessDefaultMove()
        {
            Move(defaultDestination);
        }

        private void ProcessMoveToTarget()
        {
            var target = _findMoveTarget.Found[0];
            if (!target.IsValid())
                return;

            Move(ClosestPoint(target));
        }

        private void Move(Vector2 target)
        {
            var pos = Position;
            var diff = target - pos;
            var dir = diff.normalized;
            var moveAmount = dir * (Status.speed * Time.fixedDeltaTime);

            Position += moveAmount;

            _look = target;
        }

        private void ProcessAttack()
        {
            var targets = _findAttackTarget.Found;
            if (targets.Count == 0)
                return;

            _look = targets[0].Position;
        }

        private void ProcessHit() => _attack.Process(this, _findAttackTarget.Found);

        private void ProcessDead()
        {
            if (autoRelease)
                DestroySelf();

            OnDead?.Invoke(this);
            OnDead = null;

            State = States.Release;
        }

        private void OnReleaseAttack()
        {
            if (!CheckMoveToTarget())
                State = States.Idle;
        }
        
        private void LookAt(Vector2 target)
        {
            if (_animator == null)
                return;
            
            var diff = target - Position;
            var originScale = _animator.transform.localScale;
            float originX = Mathf.Abs(originScale.x);
            float scale = diff.x <= 0 ? originX : -originX;
            _animator.transform.localScale = new Vector3(scale, originScale.y, originScale.z);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            DrawRanges();
            DrawMoveTarget();
            DrawStatus();
        }

        private void DrawRanges()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Center, Status.moveRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Center, Status.atkRange);
        }

        private void DrawMoveTarget()
        {
            if (State == States.DefaultMove)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(Position, defaultDestination);
            }
        }

        private void DrawStatus()
        {
            var style = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.green }
            };

            string text =
                $"State : {State}\n"+
                $"HP : {(float)Status.hp}/{Status.hp.Max}\n" +
                $"ATK : {(float)Status.atk}\n" +
                $"SPD : {(float)Status.speed}\n";
            
            Handles.Label(transform.position, text, style);
        }
#endif
    }
}