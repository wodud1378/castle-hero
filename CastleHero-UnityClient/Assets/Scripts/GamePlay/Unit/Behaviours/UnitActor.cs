using System;
using PolyNav;
using CastleHero.Common.Behaviours;
using CastleHero.Common.Pattern;
using CastleHero.Data.Model;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.Utility;
using CastleHero.Network.Shared;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Common;
using CastleHero.Common.Sound;

namespace CastleHero.GamePlay.Unit.Behaviours
{
    public class UnitActor : PoolItemBase, IUnitActor
    {
        private Subject<UnitActor> _onDead = new();
        public IObservable<UnitActor> OnDead => _onDead;

        public enum ActorType
        {
            Unit,
            Barricade
        }

        [field: SerializeField] public ActorType Type { get; private set; }
        [field: SerializeField] public Collider2D Collider { get; private set; }

        public IHitEffect Hit { get; private set; }
        public IEffectBody EffectBody { get; private set; }

        // Awake-time 캐싱: skills / components 가 Owner.EffectBuilder 경유로 접근 → ServiceLocator.Get 분산 방지.
        public EffectBuilder EffectBuilder { get; private set; }
        public Common.Sound.ISoundManager SoundManager { get; private set; }
        public GameConstants GameConstants { get; private set; }

        [FormerlySerializedAs("_enableAttack")]
        [SerializeField] private bool enableAttack;
        [FormerlySerializedAs("_enableMove")]
        [SerializeField] private bool enableMove;
        [FormerlySerializedAs("_enableAnimation")]
        [SerializeField] private bool enableAnimation;
        [NonSerialized] public bool autoRelease = true;

        public int Id => Data.Id;

        public Vector2 Position
        {
            get => Combat.Movement.Position;
            set => Combat.Movement.Position = value;
        }

        public Status Status => Combat.Status;
        public ReactiveProperty<UnitState.States> State => UnitState.State;
        public readonly ReactiveProperty<UnitInfo> Information = new();

        public UnitState UnitState { get; private set; }
        public CombatController Combat { get; private set; }

        public bool Released { get; private set; }

        public UnitEntity Data { get; private set; }

        private UnitBalanceEntity _balance;

        private void Awake()
        {
            var sl = ServiceLocator.Instance;
            Hit = GetComponentInChildren<IHitEffect>(true);
            EffectBody = GetComponentInChildren<IEffectBody>(true);
            EffectBuilder = sl.Get<EffectBuilder>();
            SoundManager = sl.Get<ISoundManager>();
            GameConstants = sl.Get<GameConstants>();

            this.SubscribeMessage<GameFinished>(_ =>
            {
                if (UnitState == null)
                    return;

                if (UnitState.State.Value == Components.UnitState.States.Dead)
                    return;

                UnitState.OnRest.Value = true;
            });
        }

        public void Init(UnitInfo info, UnitEntity entity, UnitBalanceEntity balance)
        {
            Information.Value = info;
            Data = entity;

            _balance = balance;

            if (UnitState == null)
            {
                UnitState = new UnitState();

                Combat = GetComponent<CombatController>();
                Combat.Construct(this, enableAttack, enableMove, enableAnimation);

                UnitState.State
                    .Where(x => x == Components.UnitState.States.Dead)
                    .Subscribe(_ => ProcessDead())
                    .AddTo(this);
            }

            UnitState.Init(info, entity);
            Combat.Init(UnitState, info, entity, balance);
            if (Hit != null)
                Hit.Init();

            this.InitAlley(entity.defLayer);

            Released = false;
        }

        public void Recovery()
        {
            _onDead.OnCompleted();
            _onDead.Dispose();
            _onDead = new Subject<UnitActor>();

            ForceActivate();
            Init(Information.Value, Data, _balance);

            Combat.Movement.Default = Position;
        }

        private void ProcessDead()
        {
            if (autoRelease)
                DestroySelf();

            if (EffectBody != null)
                EffectBody.Clear();

            _onDead.OnNext(this);

            Released = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameObject.TryGetComponent(out PolyNavAgent agent))
            {
                agent = gameObject.AddComponent<PolyNavAgent>();
            }

            if (Type == ActorType.Barricade)
            {
                agent.maxForce = 0f;
                agent.maxSpeed = 0f;
                agent.stoppingDistance = 0f;
                agent.slowingDistance = 0f;
                agent.lookAheadDistance = 0f;
            }
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

            if (Combat == null)
                return;

            DrawRanges();
            DrawStatus();
        }

        private void DrawRanges()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Position, Combat.Status.moveRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Position, Combat.Status.atkRange);
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

            var status = Combat.Status;
            string text =
                $"State : {UnitState.State}\n" +
                $"HP : {(float)status.hp}/{status.hp.Max}\n" +
                $"ATK : {(float)status.atk}\n" +
                $"SPD : {(float)status.speed}\n";

            Handles.Label(transform.position, text, style);
        }
#endif
    }
}
