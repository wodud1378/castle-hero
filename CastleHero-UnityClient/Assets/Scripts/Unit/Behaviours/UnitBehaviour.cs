using System;
using PolyNav;
using RGLabs.Common.Behaviours;
using RGLabs.Data.Model;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using UniRx;
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
        
        [field: SerializeField] public BehaviourType Type { get; private set; }
        [field: SerializeField] public Collider2D Collider { get; private set; }
        [field: SerializeField] public HitEffect Hit { get; private set; }

        [SerializeField] private bool _enableAttack;
        [SerializeField] private bool _enableMove;
        [SerializeField] private bool _enableAnimation;

        [NonSerialized] public bool autoRelease = true;

        public int Id => _data.Id;

        public Vector2 position
        {
            get => Core.navAgent.position;
            set => Core.navAgent.position = value;
        }

        public Status status => Core.status;
        public ReactiveProperty<UnitCore.States> state => Core.state;

        public bool CanMove
        {
            set => Core.canMove = value;
        }
        
        public bool CanAttack
        {
            set => Core.canAttack = value;
        }

        public UnitCore Core { get; private set; }
        
        public bool Released { get; private set; }

        private UnitEntity _data;

        private void Awake()
        {
            Core = new UnitCore(this, _enableAttack, _enableMove, _enableAnimation);
            Core.state
                .Where(x => x == UnitCore.States.Dead)
                .Subscribe(_=> ProcessDead())
                .AddTo(this);
        }

        public void Init(UnitEntity entity)
        {
            _data = entity;
            Core.SetData(entity);
            if (Hit != null)
                Hit.Init();

            this.InitAlley(entity.defLayer);

            Released = false;
        }
        
        private void ProcessDead()
        {
            if (autoRelease)
                DestroySelf();

            OnDead?.Invoke(this);
            OnDead = null;

            Released = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameObject.TryGetComponent(out PolyNavAgent agent))
            {
                agent = gameObject.AddComponent<PolyNavAgent>();
            }
            
            if (Type == BehaviourType.Barricade)
            {
                agent.maxForce = 0f;
                agent.maxSpeed = 0f;
                agent.stoppingDistance = 0f;
                agent.slowingDistance = 0f;
                agent.lookAheadDistance = 0f;
            }
            
            if (!gameObject.TryGetComponent(out CircleCollider2D collider))
                return;
                
            agent.avoidRadius = collider.radius;
            agent.centerOffset = collider.offset;
        }

        private void OnDrawGizmosSelected()
        {
            if (gameObject.TryGetComponent(out PolyNavAgent agent))
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(agent.position, agent.avoidRadius * 1.01f);
            }

            if (!Application.isPlaying)
                return;

            if (Core == null)
                return;
            
            DrawRanges();
            DrawStatus();
        }

        private void DrawRanges()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, Core.status.moveRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, Core.status.atkRange);
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

            var status = Core.status;
            string text =
                $"State : {Core.state}\n" +
                $"HP : {(float)status.hp}/{status.hp.Max}\n" +
                $"ATK : {(float)status.atk}\n" +
                $"SPD : {(float)status.speed}\n";

            Handles.Label(transform.position, text, style);
        }
#endif
    }
}