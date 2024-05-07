using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Data.Model;
using RGLabs.Unit.Components;
using RGLabs.Unit.Finding;
using RGLabs.Utility;
using Spine.Unity;
using UniRx;
using UniRx.Triggers;
using UnityEditor;
using UnityEngine;

namespace RGLabs.Unit.Behaviours
{
    public class UnitBehaviour : PoolItemBase
    {
        public event Action<UnitBehaviour> OnDead;

        public enum BehaviourType
        {
            Unit,
            Barricade
        }
        
        public enum States
        {
            Prepare,
            Idle,
            Move,
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
        };

        private static readonly int AtkSpeedHash = Animator.StringToHash("AttackSpeed");

        private const int LookFrameThreshold = 10;

        [field:SerializeField] public BehaviourType Type { get; private set; }
        [field: SerializeField] public Rigidbody2D Body { get; private set; }
        [field: SerializeField] public Collider2D Collider { get; private set; }
        [field: SerializeField] public HitEffect Hit { get; private set; }
        [field: SerializeField] public Look Look { get; private set; }

        [SerializeField] private SkeletonMecanim _skeletonMecanim;
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationEvents _animationEvents;

        [SerializeField] private int _maxAttackTarget;
        [SerializeField] private float _defaultMoveThreshlod;

        [SerializeField] private Vector2 _offset;
        [SerializeField] private bool _enableAttack;
        [SerializeField] private bool _enableMove;

        [NonSerialized] public bool autoRelease = true;
        [NonSerialized] public bool canAttack;
        [NonSerialized] public bool canMove;
        [NonSerialized] public Vector2 defaultDestination = default;

        public int Id => Data.Id;

        public UnitEntity Data { get; private set; }

        public Vector2 position
        {
            get => Body.position;
            set => Body.MovePosition(value);
        }
        
        public readonly Status status = new();

        public Vector2 Center => position + _offset;

        public readonly ReactiveProperty<States> state = new(States.Prepare);

        private FindingComponents _finding;
        private ThrustAlley _thrust;

        private RenderController _renderController;
        private Attack _attack;

        private ProjectileLauncher _projectileLauncher;

        private Vector2 _look;
        private int _currentLookFrame;

        private void Awake()
        {
            state.DistinctUntilChanged()
                .Subscribe(x =>
                {
                    switch (x)
                    {
                        case States.Idle:
                            _finding.Clear();
                            UpdateAnimation(x);
                            break;
                        case States.Dead:
                            break;
                        default:
                            UpdateAnimation(x);
                            break;
                    }
                })
                .AddTo(this);

            _thrust = new ThrustAlley(this);
            _renderController = new RenderController(_skeletonMecanim, _animator);
            
            _finding = new FindingComponents(new Collider2D[Constants.BufferSize], _maxAttackTarget);

            Collider
                .OnCollisionEnter2DAsObservable()
                .Subscribe(_thrust.Execute)
                .AddTo(this);

            _attack = new Attack(this);

            AttachAnimationEvents();
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
        
        public void Init(UnitEntity data)
        {
            state.Value = States.Prepare;
            
            Data = data;
            status.Init(data);

            if(Hit != null)
                Hit.Init();
            
            _finding.Init(UnitHelper.EnemyLayerMask(data.Id, data.atkLayer));
            this.InitAlley(data.defLayer);

            _renderController.ApplySkin(data.skinName);
            _currentLookFrame = LookFrameThreshold;

            UpdateAnimation(state.Value);

            if (!string.IsNullOrEmpty(data.projectile))
            {
                _projectileLauncher ??= new ProjectileLauncher(this, data.projectile);
            }
        }

        public Vector2 ClosestPoint(UnitBehaviour other)
        {
            var from = position;
            var closest = other.Collider.ClosestPoint(from);
            //var ranged = (closest - from).normalized * (status.atkRange * 0.9f);
            //var final = closest - ranged;
            return closest;
        }

        private void UpdateAnimation(States state)
        {
            if (!AnimationsHash.TryGetValue(state, out var hash))
                return;

            _renderController.SetAnimation(hash);
        }

        private void Update()
        {
            if (state.Value is States.Release)
                return;

            status.Update();

            UpdateState();
            ProcessState();
            UpdateLookDirection();
        }

        private void FixedUpdate()
        {
            if (state.Value == States.MoveToTarget)
            {
                ProcessMoveToTarget();
                return;
            }

            if (state.Value == States.DefaultMove)
            {
                ProcessDefaultMove();
                return;
            }
        }

        private void UpdateState()
        {
            if (state.Value == States.Prepare)
            {
                state.Value = States.Idle;
                return;
            }

            if (status.hp <= 0)
            {
                state.Value = States.Dead;
                return;
            }

            if (_enableAttack && canAttack)
            {
                if (_animator != null)
                    _animator.SetFloat(AtkSpeedHash, status.atkSpeed);
                
                if (CheckAttack())
                    return;
            }

            if (_enableMove && canMove)
            {
                if (CheckMoveToTarget())
                    return;

                if (CheckDefaultMove())
                    return;
            }

            state.Value = States.Idle;
        }

        private void ProcessState()
        {
            switch (state.Value)
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

            if(Look != null)
                Look.At(position, _look);
            
            _currentLookFrame = 0;
        }

        private bool CheckAttack()
        {
            if (state.Value == States.Attack)
                return true;

            if (_finding.attack.Update(Center, status.atkRange))
            {
                state.Value = States.Attack;
                return true;
            }

            return false;
        }

        private bool CheckMoveToTarget()
        {
            if (_finding.move.Update(Center, status.moveRange))
            {
                if (position.IsNear(_finding.move.Found[0].position))
                    return false;

                state.Value = States.MoveToTarget;
                return true;
            }

            return false;
        }

        private bool CheckDefaultMove()
        {
            if (position.IsNear(defaultDestination))
                return false;

            state.Value = States.DefaultMove;
            return true;
        }

        private void ProcessDefaultMove()
        {
            Move(defaultDestination);
        }

        private void ProcessMoveToTarget()
        {
            var target = _finding.move.Found[0];
            if (!target.IsValid())
                return;

            Move(ClosestPoint(target));
        }

        private void Move(Vector2 target)
        {
            var pos = position;
            var diff = target - pos;
            var dir = diff.normalized;
            var moveAmount = dir * (status.speed * Time.fixedDeltaTime);

            position += moveAmount;

            _look = target;
        }

        private void ProcessAttack()
        {
            var targets = _finding.attack.Found;
            if (targets.Count == 0)
                return;

            _look = targets[0].position;
        }

        private void ProcessHit()
        {
            _attack.Process(_finding.attack.Found);
            _projectileLauncher?.Launch(_finding.attack.Found);
            
            _finding.attack.Clear();
        }

        private void ProcessDead()
        {
            if (autoRelease)
                DestroySelf();

            OnDead?.Invoke(this);
            OnDead = null;

            state.Value = States.Release;
        }

        private void OnReleaseAttack()
        {
            if (!CheckAttack())
                state.Value = States.Attack;
            
            if (!CheckMoveToTarget())
                state.Value = States.Idle;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
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
            Gizmos.DrawWireSphere(Center, status.moveRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Center, status.atkRange);
        }

        private void DrawMoveTarget()
        {
            if (state.Value == States.DefaultMove)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(position, defaultDestination);
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
                $"State : {state}\n" +
                $"HP : {(float)status.hp}/{status.hp.Max}\n" +
                $"ATK : {(float)status.atk}\n" +
                $"SPD : {(float)status.speed}\n";

            Handles.Label(transform.position, text, style);
        }
#endif
    }
}