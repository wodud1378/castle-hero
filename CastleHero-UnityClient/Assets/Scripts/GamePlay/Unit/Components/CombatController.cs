using System;
using System.Collections.Generic;
using System.Linq;
using PolyNav;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components.Move;
using CastleHero.GamePlay.Unit.Events;
using CastleHero.GamePlay.Unit.Finding;
using CastleHero.GamePlay.Unit.Skill;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Components
{
    /// <summary>
    /// 전투 로직 담당 MonoBehaviour. 프리팹에 UnitActor 와 같은 GameObject 에 부착.
    /// enabled = false 상태로 시작하며, Init() 호출 시 활성화된다.
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        public enum Restrictions
        {
            Move = 0,
            Attack,
            Skill,
            Count,
        }

        public static readonly Dictionary<UnitState.States, int> AnimationsHash = new()
        {
            { UnitState.States.Idle,   Animator.StringToHash(nameof(UnitState.States.Idle)) },
            { UnitState.States.Move,   Animator.StringToHash(nameof(UnitState.States.Move)) },
            { UnitState.States.Return, Animator.StringToHash(nameof(UnitState.States.Move)) },
            { UnitState.States.Attack, Animator.StringToHash(nameof(UnitState.States.Attack)) },
            { UnitState.States.Skill,  Animator.StringToHash(nameof(UnitState.States.Skill)) },
        };

        private static readonly int AtkSpeedHash = Animator.StringToHash("AttackSpeed");

        public Status Status { get; private set; }

        public Attack Attack { get; private set; }
        public IMovement Movement { get; private set; }
        public IUnitRenderer Renderer { get; private set; }
        public IAnimationEventProvider AnimationEvent { get; private set; }

        public readonly ReactiveProperty<Vector2> LookDirection = new();

        public bool Invincible => _leftInvincible > 0f;

        private bool AllowSkill => _restrictions[(int)Restrictions.Skill] <= 0f;
        private bool AllowMove => _enableMove && _restrictions[(int)Restrictions.Move] <= 0f;
        private bool AllowAttack => _enableAttack && _restrictions[(int)Restrictions.Attack] <= 0f;

        private UnitState _state;
        private UnitActor _owner;

        private bool _enableAttack;
        private bool _enableMove;

        private float[] _restrictions;
        private IUserRepository _userRepo;

        private ISkill _skill;
        private Action _updateMethod;
        private IDisposable _update;
        private float _leftInvincible;

        /// <summary>
        /// UnitActor.Awake 에서 1 회 호출. 컴포넌트 생성 및 구독 설정.
        /// </summary>
        public void Construct(UnitActor owner, bool enableAttack, bool enableMove, bool enableAnimation)
        {
            _owner = owner;
            _enableAttack = enableAttack;
            _enableMove = enableMove;

            var sl = ServiceLocator.Instance;
            _userRepo = sl.Get<IUserRepository>();

            _restrictions = new float[(int)Restrictions.Count];

            Status = new();

            Renderer = sl.Get<IUnitRendererFactory>().Create(_owner.transform, enableAnimation);
            AnimationEvent = _owner.GetComponentInChildren<IAnimationEventProvider>(true);

            Collider2D[] buffer = null;
            if (_enableAttack)
            {
                buffer = new Collider2D[Constants.BufferSize];
                var atkFinder = Finder.Create(IDetection.Option.Circle, Constants.BufferSize, buffer);
                Attack = new Attack(_owner, atkFinder, Renderer, AnimationEvent);
            }

            if (_enableMove)
            {
                buffer ??= new Collider2D[Constants.BufferSize];
                _owner.TryGetComponent(out PolyNavAgent navAgent);
                var moveFinder = Finder.Create(IDetection.Option.Circle, 1, buffer);
                Movement = new DefaultMovement(_owner, moveFinder, navAgent);
            }
            else
                Movement = new FixedMovement();

            LookDirection
                .Subscribe(UpdateLookDirection)
                .AddTo(_owner);
        }

        /// <summary>
        /// 스테이지/로비 진입 시마다 호출. 데이터 초기화 + UpdateAsObservable 구독 시작.
        /// 첫 호출 시 State 구독도 설정한다.
        /// </summary>
        public void Init(UnitState state, UnitInfo info, UnitEntity data, UnitBalanceEntity balance)
        {
            bool firstInit = _state == null;
            _state = state;

            for (var i = 0; i < _restrictions.Length; i++)
            {
                _restrictions[i] = 0f;
            }

            if (firstInit)
            {
                _state.OnRest
                    .DistinctUntilChanged()
                    .Subscribe(OnRestStateChanged)
                    .AddTo(_owner);

                _state.State
                    .DistinctUntilChanged()
                    .Subscribe(UpdateAnimation)
                    .AddTo(_owner);
            }

            Update(info, data, balance);

            if (_enableAttack)
            {
                Attack.finder.detection.Filter = _state.EnemyLayerMask;
                Attack.projectile = data.projectile;
            }

            if (_enableMove)
                Movement.Finder.detection.Filter = _state.EnemyLayerMask;

            Renderer.ApplySkin(data.skinName);
            LookDirection.Value = Movement.Default;

            _state.State.Value = UnitState.States.Prepare;

            _update?.Dispose();
            _update = _owner
                .UpdateAsObservable()
                .Subscribe(_ => OnUpdateOwner());
        }

        public void Update(UnitInfo info, UnitEntity data, UnitBalanceEntity balance)
        {
            Status.Init(data);

            _state.Elemental.atkType = data.elementalAtk;
            _state.Elemental.defType = data.elementalDef;

            ApplyBalance(info.lv, info.rate, balance, out int skillLv);

            if (info.equipments != null)
            {
                var equipments = _userRepo.Inventory.Items
                    .OfType<EquipItem>()
                    .Where(x => info.equipments.Contains(x.Guid))
                    .ToList();

                ApplyEquipment(equipments);
            }

            if (data.skill != 0)
            {
                _skill = this.Attach(data.skill, skillLv);
            }
        }

        public void SetRestriction(Restrictions type, float duration)
        {
            _restrictions[(int)type] += duration;

            switch (type)
            {
                case Restrictions.Move:
                    Movement.Stop();
                    break;
                case Restrictions.Attack:
                    Attack?.Stop();
                    break;
            }
        }

        public void SetInvincible(float duration)
        {
            _leftInvincible += duration;
        }

        public void StopUpdate()
        {
            _update?.Dispose();
        }

        private void ApplyEquipment(List<EquipItem> equipments)
        {
            var dic = equipments.Total(ref _state.Elemental);
            foreach (var kvp in dic)
            {
                var type = kvp.Key;
                var value = kvp.Value;
                var adjustValue = Status[type].fixedAdjust;
                if (value > 0f)
                    adjustValue.Increase(value);
                else
                    adjustValue.Decrease(Mathf.Abs(value));
            }
        }

        private void ApplyBalance(int lv, int rate, UnitBalanceEntity balanceData, out int skillLv)
        {
            balanceData.AdditionalStatus(lv, rate, out var stats, out skillLv);
            if (stats == null)
                return;

            foreach (var e in stats)
            {
                Status[e.Key].fixedAdjust.Increase(e.Value);
            }
        }

        private void OnRestStateChanged(bool isRest)
        {
            _updateMethod = isRest ? OnResting : OnBattle;

            if (isRest)
            {
                Movement?.Stop();
                Attack?.Stop();
                return;
            }

            _skill?.SetToEnable();
        }

        private void OnResting()
        {
            _state.State.Value = UnitState.States.Idle;

            OnIdle();
        }

        private void OnBattle()
        {
            if (_state.State.Value == UnitState.States.Dead || _owner.Released)
                return;

            if (TrySetToDead())
                return;

            UpdateInvincible();
            UpdateRestriction();
            UpdateStatus();
            UpdateState();
            ProcessState();
        }

        private void OnUpdateOwner() => _updateMethod.Invoke();

        private bool TrySetToDead()
        {
            if (Status.hp.Left > 0)
                return false;

            _update?.Dispose();
            Attack?.Clear();
            Movement.Stop();
            _state.State.Value = UnitState.States.Dead;

            new UnitDead
            {
                unit = _owner
            }.Publish();

            return true;
        }

        private void UpdateRestriction()
        {
            int count = (int)Restrictions.Count;
            for (int i = 0; i < count; ++i)
            {
                float val = _restrictions[i] - Time.deltaTime;
                _restrictions[i] = Mathf.Clamp(val, 0f, float.MaxValue);
            }
        }

        private void UpdateInvincible()
        {
            _leftInvincible = Mathf.Clamp(_leftInvincible - Time.deltaTime, 0f, float.MaxValue);
        }

        private void UpdateStatus()
        {
            Status.Update();

            if (_enableAttack)
                Renderer.SetFloat(AtkSpeedHash, Status.atkSpeed);

            if (_enableMove)
                Movement.SetSpeed(Status.speed);
        }

        private void UpdateState()
        {
            if (Attack is { IsRunning: true })
                return;

            if (TrySetToSkill())
                return;

            if (TrySetToAttack())
                return;

            if (TrySetToMove())
                return;

            if (TrySetToReturn())
                return;

            _state.State.Value = UnitState.States.Idle;
        }

        private void ProcessState()
        {
            switch (_state.State.Value)
            {
                case UnitState.States.Prepare:
                    OnPrepare();
                    break;
                case UnitState.States.Idle:
                    OnIdle();
                    break;
                case UnitState.States.Move:
                    OnMove();
                    break;
                case UnitState.States.Return:
                    OnReturn();
                    break;
            }
        }

        private void UpdateAnimation(UnitState.States value)
        {
            if (value is not (UnitState.States.Idle or UnitState.States.Move or UnitState.States.Return))
                return;

            if (!AnimationsHash.TryGetValue(value, out var hash))
                return;

            Renderer.SetAnimation(hash);
        }

        private void UpdateLookDirection(Vector2 direction) => Renderer.ApplyLookDirection(direction - Movement.Position);

        private void OnPrepare()
        {
            Attack.Clear();
            Attack.finder.Clear();
            Movement.Finder.Clear();
            Movement.Stop();

            _state.State.Value = UnitState.States.Idle;
        }

        private bool TrySetToSkill()
        {
            if (_skill == null)
                return false;

            if (!AllowSkill)
                return false;

            if (_skill.Runner.IsRunning)
                return true;

            if (_skill.Cycle.IsReady)
            {
                _skill.Runner.Run();

                _owner.SoundManager.PlaySfx(_skill.Data.sfx);
                _state.State.Value = UnitState.States.Skill;
                return true;
            }

            return false;
        }

        private bool TrySetToAttack()
        {
            if (!AllowAttack || !Attack.IsAbleToAttack())
                return false;

            if (Attack.IsRunning)
                return true;

            LookDirection.Value = Attack.CurrentTarget.Position;

            Attack.Run();
            Movement.Stop();
            _state.State.Value = UnitState.States.Attack;
            return true;
        }

        private bool TrySetToMove()
        {
            if (!AllowMove)
                return false;

            if (Movement.TryMoveToTarget())
            {
                _state.State.Value = UnitState.States.Move;
                return true;
            }

            return false;
        }

        private bool TrySetToReturn()
        {
            if (!AllowMove)
                return false;

            if (Movement.TryMoveToDefault())
            {
                _state.State.Value = UnitState.States.Return;
                return true;
            }

            return false;
        }

        private void OnIdle()
        {
            LookDirection.Value = Movement.Position * 2f;
        }

        private void OnMove()
        {
            LookDirection.Value = Movement.CurrentTarget.Position;

            TrySetToAttack();
        }

        private void OnReturn()
        {
            LookDirection.Value = Movement.Default;

            if (TrySetToAttack())
                return;

            TrySetToMove();
        }
    }
}
