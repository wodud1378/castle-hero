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
    public class UnitCore
    {
        public enum Teams
        {
            Monster,
            Character,
        }

        public enum States
        {
            Prepare,
            Idle,
            Move,
            Return,
            Attack,
            Skill,
            Dead,
        }

        public enum Restrictions
        {
            Move = 0,
            Attack,
            Skill,
            Count,
        }

        public static readonly Dictionary<States, int> AnimationsHash = new()
        {
            { States.Idle,   Animator.StringToHash(nameof(States.Idle)) },
            { States.Move,   Animator.StringToHash(nameof(States.Move)) },
            { States.Return, Animator.StringToHash(nameof(States.Move)) },
            { States.Attack, Animator.StringToHash(nameof(States.Attack)) },
            { States.Skill,  Animator.StringToHash(nameof(States.Skill)) },
        };

        private static readonly int AtkSpeedHash = Animator.StringToHash("AttackSpeed");

        public readonly ReactiveProperty<bool> OnRest;
        public readonly ReactiveProperty<States> State;
        public readonly Status Status;

        public readonly UnitBehaviour Owner;
        public readonly Attack Attack;
        public readonly IMovement Movement;
        public readonly IUnitRenderer Renderer;
        public readonly IAnimationEventProvider AnimationEvent;
        public readonly ReactiveProperty<Vector2> LookDirection = new();

        public bool EnableRecover;

        private readonly bool _enableAttack;
        private readonly bool _enableMove;

        public Elemental Elemental;
        public LayerMask EnemyLayerMask;
        public LayerMask AlleyLayerMask;

        private readonly float[] _restrictions;

        public Teams Team { get; private set; }

        public bool Invincible => _leftInvincible > 0f;

        private bool AllowSkill => _restrictions[(int)Restrictions.Skill] <= 0f;
        private bool AllowMove => _enableMove && _restrictions[(int)Restrictions.Move] <= 0f;
        private bool AllowAttack => _enableAttack && _restrictions[(int)Restrictions.Attack] <= 0f;

        private ISkill _skill;
        private Action _updateMethod;
        private IDisposable _update;
        private float _leftInvincible;

        public UnitCore(UnitBehaviour owner, bool enableAttack, bool enableMove, bool enableAnimation)
        {
            _restrictions = new float[(int)Restrictions.Count];

            Status = new();

            Owner = owner;
            _enableAttack = enableAttack;
            _enableMove = enableMove;

            Elemental = new();

            // Renderer: View 구현체를 Factory 경유로 획득 (GamePlay 는 View 타입 직접 참조 없음).
            Renderer = ServiceLocator.Get<IUnitRendererFactory>().Create(Owner.transform, enableAnimation);
            // AnimationEvent: AnimationEvents(View MonoBehaviour) 는 자식 GameObject 에 붙어 있음.
            AnimationEvent = Owner.GetComponentInChildren<IAnimationEventProvider>(true);

            Collider2D[] buffer = null;
            if (_enableAttack)
            {
                buffer = new Collider2D[Constants.BufferSize];
                var atkFinder = Finder.Create(IDetection.Option.Circle, Constants.BufferSize, buffer);
                Attack = new Attack(Owner, atkFinder, Renderer, AnimationEvent);
            }

            if (_enableMove)
            {
                buffer ??= new Collider2D[Constants.BufferSize];
                Owner.TryGetComponent(out PolyNavAgent navAgent);
                var moveFinder = Finder.Create(IDetection.Option.Circle, 1, buffer);
                Movement = new DefaultMovement(Owner, moveFinder, navAgent);
            }
            else
                Movement = new FixedMovement();

            OnRest = new();
            OnRest
                .DistinctUntilChanged()
                .Subscribe(OnRestStateChanged)
                .AddTo(Owner);

            State = new(States.Prepare);
            State
                .DistinctUntilChanged()
                .Subscribe(UpdateAnimation)
                .AddTo(Owner);

            LookDirection
                .Subscribe(UpdateLookDirection)
                .AddTo(Owner);
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

            // TODO : 이펙트?
        }

        public void Init(UnitInfo info, UnitEntity data, UnitBalanceEntity balance)
        {
            for (var i = 0; i < _restrictions.Length; i++)
            {
                _restrictions[i] = 0f;
            }

            Team = data.Id / 10000 == 1 ? Teams.Character : Teams.Monster;
            EnemyLayerMask = UnitHelper.EnemyLayerMask(data.Id, data.atkLayer);
            AlleyLayerMask = UnitHelper.AlleyLayerMask(data.Id);

            Update(info, data, balance);

            if (_enableAttack)
            {
                Attack.finder.detection.Filter = EnemyLayerMask;
                Attack.projectile = data.projectile;
            }

            if (_enableMove)
                Movement.Finder.detection.Filter = EnemyLayerMask;

            Renderer.ApplySkin(data.skinName);
            LookDirection.Value = Movement.Default;

            State.Value = States.Prepare;

            _update?.Dispose();
            _update = Owner
                .UpdateAsObservable()
                .Subscribe(_ => OnUpdateOwner());
        }

        public void Update(UnitInfo info, UnitEntity data, UnitBalanceEntity balance)
        {
            Status.Init(data);

            Elemental.atkType = data.elementalAtk;
            Elemental.defType = data.elementalDef;
            
            ApplyBalance(info.lv, info.rate, balance, out int skillLv);
            
            if (info.equipments != null)
            {
                var equipments = ServiceLocator.Get<IUserRepository>().Inventory.Items
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
        
        private void ApplyEquipment(List<EquipItem> equipments)
        {
            var dic = equipments.Total(ref Elemental);
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
            State.Value = States.Idle;

            OnIdle();
        }

        private void OnBattle()
        {
            if (State.Value == States.Dead || Owner.Released)
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
            State.Value = States.Dead;

            new UnitDead
            {
                unit = Owner
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

            State.Value = States.Idle;
        }

        private void ProcessState()
        {
            switch (State.Value)
            {
                case States.Prepare:
                    OnPrepare();
                    break;
                case States.Idle:
                    OnIdle();
                    break;
                case States.Move:
                    OnMove();
                    break;
                case States.Return:
                    OnReturn();
                    break;
            }
        }

        private void UpdateAnimation(States value)
        {
            if (value is not (States.Idle or States.Move or States.Return))
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

            State.Value = States.Idle;
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

                Owner.SoundManager.PlaySfx(_skill.Data.sfx);
                State.Value = States.Skill;
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
            State.Value = States.Attack;
            return true;
        }

        private bool TrySetToMove()
        {
            if (!AllowMove)
                return false;

            if (Movement.TryMoveToTarget())
            {
                State.Value = States.Move;
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
                State.Value = States.Return;
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