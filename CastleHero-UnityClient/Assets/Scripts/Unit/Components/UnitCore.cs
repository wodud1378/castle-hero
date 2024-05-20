using System;
using System.Collections.Generic;
using PolyNav;
using RGLabs.Common;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
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

        public static readonly Dictionary<States, int> AnimationsHash = new()
        {
            { States.Idle, Animator.StringToHash("Idle") },
            { States.Move, Animator.StringToHash("Move") },
            { States.Return, Animator.StringToHash("Move") },
            { States.Attack, Animator.StringToHash("Attack") },
            { States.Skill, Animator.StringToHash("Skill") },
        };

        private static readonly int AtkSpeedHash = Animator.StringToHash("AttackSpeed");

        public readonly ReactiveProperty<States> state;
        public readonly Status status;
        public readonly PolyNavAgent navAgent;
        public readonly Elemental elemental;

        public readonly UnitBehaviour owner;
        public readonly Look look;
        public readonly Attack attack;
        public readonly RenderController renderController;
        public readonly FindingComponents finding;
        public readonly AnimationEvents animationEvent;

        private readonly bool _enableAttack;
        private readonly bool _enableMove;
        private readonly bool _enableAnimation;
        
        public bool Invincible => _leftInvincible > 0f;
        
        public bool canMove
        {
            get => navAgent.enabled;
            set => navAgent.enabled = value;
        }

        public bool canAttack;

        public Vector2 defaultDestination;

        private readonly ReactiveProperty<Vector2> _lookDirection;

        private bool AllowMove => _enableMove && canMove;
        private bool AllowAttack => _enableAttack && canAttack;

        private ISkill _skill;
        private IDisposable _update;
        private float _leftInvincible;

        public UnitCore(UnitBehaviour owner, bool enableAttack, bool enableMove, bool enableAnimation)
        {
            status = new();

            this.owner = owner;
            _enableAttack = enableAttack;
            _enableMove = enableMove;
            _enableAnimation = enableAnimation;

            elemental = new();
            navAgent = this.owner.GetComponent<PolyNavAgent>();
            finding = new FindingComponents(new Collider2D[Constants.BufferSize], status);

            animationEvent = this.owner.GetComponentInChildren<AnimationEvents>();
            if (_enableAttack)
                attack = new Attack(this.owner, finding.attack, animationEvent);

            var skeleton = this.owner.GetComponentInChildren<SkeletonMecanim>();
            var animator = this.owner.GetComponentInChildren<Animator>();
            renderController = new RenderController(skeleton, animator, _enableAnimation);
            look = new Look(animator.transform);

            state = new(States.Prepare);
            state
                .DistinctUntilChanged()
                .Subscribe(UpdateAnimation)
                .AddTo(this.owner);

            _lookDirection = new();
            _lookDirection
                .Subscribe(UpdateLookDirection)
                .AddTo(this.owner);
        }

        public void SetInvincible(float duration)
        {
            _leftInvincible += duration;

            // TODO : 이펙트?
        }

        private void OnUpdateOwner()
        {
            if (state.Value == States.Dead || owner.Released)
                return;

            if (TrySetToDead())
                return;

            UpdateInvincible();
            UpdateStatus();
            UpdateState();
            ProcessState();
        }

        private bool TrySetToDead()
        {
            if (status.hp.Left > 0)
                return false;

            _update?.Dispose();
            state.Value = States.Dead;

            if (status.recovery > 0f)
            {
                new WaitRecover
                {
                    behaviour = owner,
                    position = defaultDestination,
                    leftTime = status.recovery
                }.Publish();
            }

            return true;
        }

        private void UpdateInvincible()
        {
            _leftInvincible = Mathf.Clamp(_leftInvincible - Time.deltaTime, 0f, float.MaxValue);
        }

        public void SetData(UnitEntity data, int lv, UnitLevelEntity levelData, SkillEntity skillData)
        {
            status.Init(data, lv, levelData);
            elemental.atkType = (Elemental.Type)data.elementalAtk;
            elemental.defType = (Elemental.Type)data.elementalDef;

            finding.Init(UnitHelper.EnemyLayerMask(data.Id, data.atkLayer));
            renderController.ApplySkin(data.skinName);
            UpdateLookDirection(defaultDestination);

            if (!string.IsNullOrEmpty(data.projectile) && attack != null)
            {
                attack.projectileLauncher ??= new ProjectileLauncher(owner, data.projectile);
            }

            if (skillData.Id != 0)
            {
                // TODO : ID + 스킬레벨
                var type = Type.GetType($"RGLabs.Unit.Skill.Skill{skillData.Id}");
                if (type != null)
                    _skill = (ISkill)Activator.CreateInstance(type, this, skillData);
            }

            state.Value = States.Prepare;

            _update = owner
                .UpdateAsObservable()
                .Subscribe(_ => OnUpdateOwner())
                .AddTo(owner);
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
            if (attack is { InProgress: true })
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
            if (!_enableAnimation)
                return;

            if (!AnimationsHash.TryGetValue(value, out var hash))
                return;

            renderController.SetAnimation(hash);
        }

        private void UpdateLookDirection(Vector2 direction) => look.At(navAgent.position, direction);

        private void OnPrepare()
        {
            finding.Clear();

            state.Value = States.Idle;
        }

        private void OnIdle()
        {
            if (TrySetToSkill()) return;
            if (TrySetToAttack()) return;
            if (TrySetToMove()) return;
            if (TrySetToReturn()) return;
        }

        private bool TrySetToSkill()
        {
            if (_skill == null)
                return false;

            if (_skill.State.Value != SkillState.Ready)
                return false;
            
            _skill.Run();
            return true;
        }

        private bool TrySetToAttack()
        {
            if (!AllowAttack)
                return false;

            if (!finding.IsAbleToAttack(navAgent.position))
                return false;

            if (state.Value == States.Attack)
                return true;

            state.Value = States.Attack;
            _lookDirection.Value = finding.attack.Found[0].position;

            attack.Execute();
            navAgent.Stop();
            return true;
        }

        private bool TrySetToMove()
        {
            if (!AllowMove)
                return false;

            var position = navAgent.position;
            if (!finding.TryFindMoveTarget(position, out var target))
                return false;

            state.Value = States.Move;
            navAgent.SetDestination(target.position);
            return true;
        }

        private bool TrySetToReturn()
        {
            if (!AllowMove)
                return false;

            if ((defaultDestination - navAgent.position).magnitude < navAgent.stoppingDistance)
                return false;

            if (state.Value == States.Return)
                return true;

            state.Value = States.Return;
            navAgent.SetDestination(defaultDestination);
            return true;
        }

        private void OnMove()
        {
            _lookDirection.Value = finding.move.Found[0].position;

            TrySetToAttack();
        }

        private void OnReturn()
        {
            _lookDirection.Value = defaultDestination;

            if (TrySetToAttack())
                return;

            TrySetToMove();
        }
    }
}