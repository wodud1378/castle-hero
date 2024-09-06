using System;
using System.Collections.Generic;
using System.Linq;
using PolyNav;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Network.Shared;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components.Move;
using RGLabs.Unit.Finding;
using RGLabs.Unit.Skill;
using RGLabs.Utility;
using Spine.Unity;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Components
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
            { States.Idle, Animator.StringToHash("Idle") },
            { States.Move, Animator.StringToHash("Move") },
            { States.Return, Animator.StringToHash("Move") },
            { States.Attack, Animator.StringToHash("Attack") },
            { States.Skill, Animator.StringToHash("Skill") },
        };

        private static readonly int AtkSpeedHash = Animator.StringToHash("AttackSpeed");

        public readonly ReactiveProperty<bool> onRest;
        public readonly ReactiveProperty<States> state;
        public readonly Status status;
        public readonly PolyNavAgent navAgent;

        public readonly UnitBehaviour owner;
        public readonly Look look;
        public readonly Attack attack;
        public readonly IMovement movement;
        public readonly RenderController renderController;
        public readonly AnimationEvents animationEvent;
        public readonly ReactiveProperty<Vector2> lookDirection;

        public bool enableRecover;

        private readonly bool _enableAttack;
        private readonly bool _enableMove;

        public Elemental elemental;
        public LayerMask enemyLayerMask;
        public LayerMask alleyLayerMask;

        public readonly float[] restrictions;

        public Teams Team { get; private set; }

        public bool Invincible => _leftInvincible > 0f;

        private bool AllowSkill => restrictions[(int)Restrictions.Skill] <= 0f;
        private bool AllowMove => _enableMove && restrictions[(int)Restrictions.Move] <= 0f;
        private bool AllowAttack => _enableAttack && restrictions[(int)Restrictions.Attack] <= 0f;

        private ISkill _skill;
        private Action _updateMethod;
        private IDisposable _update;
        private float _leftInvincible;

        public UnitCore(UnitBehaviour owner, bool enableAttack, bool enableMove, bool enableAnimation)
        {
            restrictions = new float[(int)Restrictions.Count];

            status = new();

            this.owner = owner;
            _enableAttack = enableAttack;
            _enableMove = enableMove;

            elemental = new();
            navAgent = this.owner.GetComponent<PolyNavAgent>();

            var skeleton = this.owner.GetComponentInChildren<SkeletonMecanim>();
            var animator = this.owner.GetComponentInChildren<Animator>();
            if(animator != null)
                look = new Look(animator.transform);
            
            renderController = new RenderController(skeleton, animator, enableAnimation);
            animationEvent = this.owner.GetComponentInChildren<AnimationEvents>();

            Collider2D[] buffer = null;
            if (_enableAttack)
            {
                buffer = new Collider2D[Constants.BufferSize];
                var atkFinder = Finder.Create(IDetection.Option.Circle, Constants.BufferSize, buffer);
                attack = new Attack(this.owner, atkFinder, renderController, animationEvent);
            }

            if (_enableMove)
            {
                buffer ??= new Collider2D[Constants.BufferSize];
                var moveFinder = Finder.Create(IDetection.Option.Circle, 1, buffer);
                movement = new DefaultMovement(this.owner, moveFinder, navAgent);
            }
            else
                movement = new FixedMovement();

            onRest = new();
            onRest
                .DistinctUntilChanged()
                .Subscribe(OnRestStateChanged)
                .AddTo(this.owner);

            state = new(States.Prepare);
            state
                .DistinctUntilChanged()
                .Subscribe(UpdateAnimation)
                .AddTo(this.owner);

            lookDirection = new();
            lookDirection
                .Subscribe(UpdateLookDirection)
                .AddTo(this.owner);
        }

        public void SetInvincible(float duration)
        {
            _leftInvincible += duration;

            // TODO : 이펙트?
        }

        public void Init(UnitInfo info, UnitEntity data, UnitBalanceEntity balance)
        {
            for (var i = 0; i < restrictions.Length; i++)
            {
                restrictions[i] = 0f;
            }
            
            Team = data.Id / 10000 == 1 ? Teams.Character : Teams.Monster;
            enemyLayerMask = UnitHelper.EnemyLayerMask(data.Id, data.atkLayer);
            alleyLayerMask = UnitHelper.AlleyLayerMask(data.Id);
            
            Update(info, data, balance);

            if (_enableAttack)
            {
                attack.finder.detection.Filter = enemyLayerMask;
                attack.projectile = data.projectile;
            }

            if (_enableMove)
                movement.Finder.detection.Filter = enemyLayerMask;

            renderController.ApplySkin(data.skinName);
            UpdateLookDirection(movement.Default);
            
            state.Value = States.Prepare;
            
            _update?.Dispose();
            _update = owner
                .UpdateAsObservable()
                .Subscribe(_ => OnUpdateOwner())
                .AddTo(owner);
        }

        public void Update(UnitInfo info, UnitEntity data, UnitBalanceEntity balance)
        {
            status.Init(data);
            
            elemental.atkType = data.elementalAtk;
            elemental.defType = data.elementalDef;
            
            ApplyBalance(info.lv, info.rate, balance, out int skillLv);
            
            if (info.equipments != null)
            {
                var equipments = Storage.userRepository.inventory.items
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
            var dic = equipments.Total(ref elemental);
            foreach (var kvp in dic)
            {
                var type = kvp.Key;
                var value = kvp.Value;
                var adjustValue = status[type].fixedAdjust;
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
                status[e.Key].fixedAdjust.Increase(e.Value);
            }
        }

        private void OnRestStateChanged(bool isRest)
        {
            _updateMethod = isRest ? OnRest : OnBattle;

            if (isRest)
                return;

            _skill?.SetToEnable();
        }

        private void OnRest()
        {
            state.Value = States.Idle;

            OnIdle();
        }

        private void OnBattle()
        {
            if (state.Value == States.Dead || owner.Released)
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
            if (status.hp.Left > 0)
                return false;

            _update?.Dispose();
            attack?.Clear();
            navAgent.Stop();
            state.Value = States.Dead;

            new UnitDead
            {
                unit = owner
            }.Publish();

            return true;
        }

        private void UpdateRestriction()
        {
            int count = (int)Restrictions.Count;
            for (int i = 0; i < count; ++i)
            {
                float val = restrictions[i] - Time.deltaTime;
                restrictions[i] -= Mathf.Clamp(val, 0f, float.MaxValue);
            }
        }

        private void UpdateInvincible()
        {
            _leftInvincible = Mathf.Clamp(_leftInvincible - Time.deltaTime, 0f, float.MaxValue);
        }

        private void UpdateStatus()
        {
            status.Update();

            if (_enableAttack)
                renderController.SetFloat(AtkSpeedHash, status.atkSpeed);

            if (_enableMove)
                navAgent.maxSpeed = status.speed;
        }

        private void UpdateState()
        {
            if (attack is { IsRunning: true })
                return;

            if (TrySetToSkill())
                return;

            if (TrySetToAttack())
                return;

            if (TrySetToMove())
                return;

            if (TrySetToReturn())
                return;

            state.Value = States.Idle;
        }

        private void ProcessState()
        {
            switch (state.Value)
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

            renderController.SetAnimation(hash);
        }

        private void UpdateLookDirection(Vector2 direction) => look.At(navAgent.position, direction);

        private void OnPrepare()
        {
            attack.Clear();
            attack.finder.Clear();
            movement.Finder.Clear();
            movement.Stop();

            state.Value = States.Idle;
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
                
                Context.sounds.PlaySfx(_skill.Data.sfx);
                state.Value = States.Skill;
                return true;
            }

            return false;
        }

        private bool TrySetToAttack()
        {
            if (!AllowAttack || !attack.IsAbleToAttack())
                return false;

            if (attack.IsRunning)
                return true;

            lookDirection.Value = attack.finder.Found[0].position;

            attack.Run();
            movement.Stop();
            state.Value = States.Attack;
            return true;
        }

        private bool TrySetToMove()
        {
            if (!AllowMove)
                return false;

            if (movement.TryMoveToTarget())
            {
                state.Value = States.Move;
                return true;
            }

            return false;
        }

        private bool TrySetToReturn()
        {
            if (!AllowMove)
                return false;

            if (movement.TryMoveToDefault())
            {
                state.Value = States.Return;
                return true;
            }

            return false;
        }

        private void OnIdle()
        {
            lookDirection.Value = navAgent.position * 2f;
        }

        private void OnMove()
        {
            lookDirection.Value = movement.CurrentTarget.position;

            TrySetToAttack();
        }

        private void OnReturn()
        {
            lookDirection.Value = movement.Default;

            if (TrySetToAttack())
                return;

            TrySetToMove();
        }
    }
}