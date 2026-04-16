using System;
using PolyNav;
using CastleHero.Common.Behaviours;
using CastleHero.Common.Pattern;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.InGame;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.Utility;
using CastleHero.Network.Shared;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

using CastleHero.Common;
namespace CastleHero.GamePlay.Unit.Behaviours
{
    public class UnitBehaviour : PoolItemBase, IUnitBehaviour
    {
        public event Action<UnitBehaviour> OnDead;

        public enum BehaviourType
        {
            Unit,
            Barricade
        }

        [field: SerializeField] public BehaviourType Type { get; private set; }
        [field: SerializeField] public Collider2D Collider { get; private set; }

        public IHitEffect Hit { get; private set; }
        public IEffectBody EffectBody { get; private set; }

        // Awake-time 캐싱: skills / components 가 Owner.EffectBuilder 경유로 접근 → ServiceLocator.Get 분산 방지.
        public EffectBuilder EffectBuilder { get; private set; }
        public CastleHero.Common.Sound.ISoundManager SoundManager { get; private set; }
        public CastleHero.Common.GameConstants GameConstants { get; private set; }

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
            get => Core.Movement.Position;
            set => Core.Movement.Position = value;
        }

        public Status Status => Core.Status;
        public ReactiveProperty<UnitCore.States> State => Core.State;
        public readonly ReactiveProperty<UnitInfo> Information = new();

        public UnitCore Core { get; private set; }

        public bool Released { get; private set; }

        public UnitEntity Data { get; private set; }

        private UnitBalanceEntity _balance;

        private void Awake()
        {
            Hit = GetComponentInChildren<IHitEffect>(true);
            EffectBody = GetComponentInChildren<IEffectBody>(true);
            EffectBuilder = ServiceLocator.Get<EffectBuilder>();
            SoundManager = ServiceLocator.Get<CastleHero.Common.Sound.ISoundManager>();
            GameConstants = ServiceLocator.Get<CastleHero.Common.GameConstants>();

            this.SubscribeMessage<GameFinished>(_ =>
            {
                if (Core.State.Value == UnitCore.States.Dead)
                    return;

                Core.OnRest.Value = true;
            });
        }

        public void Init(UnitInfo info, UnitEntity entity, UnitBalanceEntity balance)
        {
            Information.Value = info;
            Data = entity;

            _balance = balance;

            if (Core == null)
            {
                Core = new UnitCore(this, enableAttack, enableMove, enableAnimation);
                Core.State
                    .Where(x => x == UnitCore.States.Dead)
                    .Subscribe(_ => ProcessDead())
                    .AddTo(this);
            }

            Core.Init(info, entity, balance);
            if (Hit != null)
                Hit.Init();

            this.InitAlley(entity.defLayer);

            Released = false;
        }

        public void Recovery()
        {
            ForceActivate();
            Init(Information.Value, Data, _balance);

            Core.Movement.Default = Position;
        }

        private void ProcessDead()
        {
            if (autoRelease)
                DestroySelf();

            if (EffectBody != null)
                EffectBody.Clear();

            OnDead?.Invoke(this);
            OnDead = null;

            if (Core.Team == UnitCore.Teams.Monster)
            {
                EffectBuilder.Run(GameConstants.deadEffect, Position);
                EffectBuilder.Run(GameConstants.manaDropEffect, Position);

                // int 랜덤은 맥스값 - 1, 가독성을 위해 +1.
                InGameSession.Current.Mana.Value += UnityEngine.Random.Range(3, 10 + 1);
            }

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
            Gizmos.DrawWireSphere(Position, Core.Status.moveRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Position, Core.Status.atkRange);
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

            var status = Core.Status;
            string text =
                $"State : {Core.State}\n" +
                $"HP : {(float)status.hp}/{status.hp.Max}\n" +
                $"ATK : {(float)status.atk}\n" +
                $"SPD : {(float)status.speed}\n";

            Handles.Label(transform.position, text, style);
        }
#endif
    }
}
