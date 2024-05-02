using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Data.Model;
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
        };

        private static readonly int AtkSpeedHash = Animator.StringToHash("AttackSpeed");

        private const int LookFrameThreshold = 10;

        [SerializeField] private SkeletonMecanim _skeletonMecanim;
        [field: SerializeField] public Rigidbody2D Body { get; private set; }
        [field: SerializeField] public Collider2D Collider { get; private set; }
        [field: SerializeField] public HitEffect Hit { get; private set; }

        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationEvents _animationEvents;

        [SerializeField] private int _maxAttackTarget;
        [SerializeField] private float _defaultMoveThreshlod;

        [SerializeField] private Vector2 _offset;

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

        private Vector2 Center => position + _offset;

        public readonly ReactiveProperty<States> state = new(States.Prepare);

        private readonly Collider2D[] _castBuffer = new Collider2D[Constants.BufferSize];

        [SerializeField] private FindMoveTarget _findMoveTarget;
        [SerializeField] private FindAttackTarget _findAttackTarget;

        private RenderController _renderController;
        private Attack _attack;

        private LayerMask _enemyLayer;

        private Vector2 _look;
        private int _currentLookFrame;

        public void Init(UnitEntity data)
        {
            state.Value = States.Prepare;
            
            Data = data;
            status.Init(data);

            InitAlley(data.Id, data.defLayer);
            InitEnemy(data.Id, data.atkLayer);

            _renderController.ApplySkin(data.skinName);
            _currentLookFrame = LookFrameThreshold;

            UpdateAnimation(state.Value);
        }

        private void InitAlley(int id, int defLayer)
        {
            var alleyTag = AlleyTag(id);
            var alleyLayer = DefTypeToLayer(alleyTag, defLayer);
            var go = gameObject;

            go.tag = alleyTag;
            go.layer = alleyLayer;
        }

        private void InitEnemy(int id, int atkLayer)
        {
            var enemyLayerMask = EnemyLayerMask(id, atkLayer);
            InitFindUnitComponent(_findMoveTarget, enemyLayerMask);
            InitFindUnitComponent(_findAttackTarget, enemyLayerMask);
        }

        private string AlleyTag(int id) => id.ToString().StartsWith("1") ? "Character" : "Monster";

        private string EnemyTag(int id) => id.ToString().StartsWith("1") ? "Monster" : "Character";

        private LayerMask EnemyLayerMask(int id, int atkType)
        {
            LayerMask layerMask = default;

            string tag = EnemyTag(id);
            int groundUnit = 1 << DefTypeToLayer(tag, 1);
            int flightUnit = 2 << DefTypeToLayer(tag, 2);
            switch (atkType)
            {
                case 0:
                    layerMask = groundUnit | flightUnit;
                    break;
                case 1:
                    layerMask = groundUnit;
                    break;
                case 2:
                    layerMask = flightUnit;
                    break;
            }

            return layerMask;
        }

        private int DefTypeToLayer(string tag, int defType)
        {
            string type = defType switch
            {
                1 => "Ground",
                2 => "Flight",
                _ => string.Empty
            };

            return LayerMask.NameToLayer($"{type}{tag}");
        }

        private void InitFindUnitComponent(FindUnits component, LayerMask layerMask)
        {
            component.layerMask = layerMask;
            component.Clear();
        }

        public Vector2 ClosestPoint(UnitBehaviour other)
        {
            var from = position;
            var closest = other.Collider.ClosestPoint(from);
            //var ranged = (closest - from).normalized * (status.atkRange * 0.9f);
            //var final = closest - ranged;
            return closest;
        }

        private void Awake()
        {
            state.DistinctUntilChanged()
                .Subscribe(x =>
                {
                    switch (x)
                    {
                        case States.Idle:
                            _findMoveTarget.Clear();
                            _findAttackTarget.Clear();
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

            _findMoveTarget = new FindMoveTarget(_castBuffer, 1);
            _findAttackTarget = new FindAttackTarget(_castBuffer, _maxAttackTarget);
            _renderController = new RenderController(_skeletonMecanim, _animator);

            Collider
                .OnCollisionEnter2DAsObservable()
                .Subscribe(ThrustAlley)
                .AddTo(this);

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

            if (canAttack)
            {
                if (_animator != null)
                    _animator.SetFloat(AtkSpeedHash, status.atkSpeed);
                
                if (CheckAttack())
                    return;
            }

            if (canMove)
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

            LookAt(_look);
            _currentLookFrame = 0;
        }

        private bool CheckAttack()
        {
            if (state.Value == States.Attack)
                return true;

            if (_findAttackTarget.Update(Center, status.atkRange))
            {
                state.Value = States.Attack;
                return true;
            }

            return false;
        }

        private bool CheckMoveToTarget()
        {
            if (state.Value == States.MoveToTarget)
                return true;

            if (_findMoveTarget.Update(Center, status.moveRange))
            {
                if (position.IsNear(_findMoveTarget.Found[0].position, 0f))
                    return false;

                state.Value = States.MoveToTarget;
                return true;
            }

            return false;
        }

        private bool CheckDefaultMove()
        {
            if (position.IsNear(defaultDestination, _defaultMoveThreshlod))
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
            var target = _findMoveTarget.Found[0];
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

        private void ThrustAlley(Collision2D collision)
        {
            foreach (var contact in collision.contacts)
            {
                var obj = contact.collider.gameObject;
                if (!obj.CompareTag(gameObject.tag))
                    continue;

                if (obj.layer != gameObject.layer)
                    continue;

                var rigidbody = contact.rigidbody;
                if (rigidbody == null)
                    continue;

                var point = contact.point - position;
                var direction = point.normalized;
                rigidbody.AddForceAtPosition(direction * 1.5f, point, ForceMode2D.Force);
            }
        }

        private void ProcessAttack()
        {
            var targets = _findAttackTarget.Found;
            if (targets.Count == 0)
                return;

            _look = targets[0].position;
        }

        private void ProcessHit()
        {
            _attack.Process(this, _findAttackTarget.Found);
            
            _findAttackTarget.Clear();
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
            if (!CheckMoveToTarget())
                state.Value = States.Idle;
        }

        private void LookAt(Vector2 target)
        {
            if (_animator == null)
                return;

            var diff = target - position;
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