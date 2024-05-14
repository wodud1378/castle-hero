using System;
using System.Collections.Generic;
using PolyNav;
using RGLabs.Common;
using RGLabs.Data.Model;
using RGLabs.Data.User;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
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
            Dead,
        }

        private static readonly Dictionary<States, int> AnimationsHash = new()
        {
            { States.Idle, Animator.StringToHash("Idle") },
            { States.Move, Animator.StringToHash("Move") },
            { States.Return, Animator.StringToHash("Move") },
            { States.Attack, Animator.StringToHash("Attack") },
        };

        private static readonly int AtkSpeedHash = Animator.StringToHash("AttackSpeed");

        public readonly ReactiveProperty<States> state;
        public readonly Status status;
        public readonly PolyNavAgent navAgent;
        public readonly Elemental elemental;

        private readonly UnitBehaviour _owner;
        private readonly Look _look;
        private readonly Attack _attack;
        private readonly RenderController _renderController;
        private readonly FindingComponents _finding;

        private readonly bool _enableAttack;
        private readonly bool _enableMove;
        private readonly bool _enableAnimation;

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

        private IDisposable _update;
        
        public UnitCore(UnitBehaviour owner, bool enableAttack, bool enableMove, bool enableAnimation)
        {
            status = new();
            
            _owner = owner;
            _enableAttack = enableAttack;
            _enableMove = enableMove;
            _enableAnimation = enableAnimation;

            elemental = new();
            navAgent = _owner.GetComponent<PolyNavAgent>();
            _finding = new FindingComponents(new Collider2D[Constants.BufferSize], status);

            if (_enableAttack)
                _attack = new Attack(_owner, _finding.attack, _owner.GetComponentInChildren<AnimationEvents>());

            var skeleton = _owner.GetComponentInChildren<SkeletonMecanim>();
            var animator = _owner.GetComponentInChildren<Animator>();
            _renderController = new RenderController(skeleton, animator, _enableAnimation);
            _look = new Look(animator.transform);

            state = new(States.Prepare);
            state
                .DistinctUntilChanged()
                .Subscribe(UpdateAnimation)
                .AddTo(_owner);

            _lookDirection = new();
            _lookDirection
                .Subscribe(UpdateLookDirection)
                .AddTo(_owner);
        }

        private void OnUpdateOwner()
        {
            if (state.Value == States.Dead || _owner.Released)
                return;
            
            if (TrySetToDead())
                return;

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
                    behaviour = _owner,
                    position = defaultDestination,
                    time = status.recovery
                }.Publish();
            }
            
            return true;
        }

        public void SetData(UnitEntity data)
        {
            status.Init(data);
            elemental.atkType = (Elemental.Type)data.elementalAtk;
            elemental.defType = (Elemental.Type)data.elementalDef;

            _finding.Init(UnitHelper.EnemyLayerMask(data.Id, data.atkLayer));
            _renderController.ApplySkin(data.skinName);
            UpdateLookDirection(defaultDestination);

            if (!string.IsNullOrEmpty(data.projectile) && _attack != null)
            {
                _attack.projectileLauncher ??= new ProjectileLauncher(_owner, data.projectile);
            }

            state.Value = States.Prepare;

            _update = _owner
                .UpdateAsObservable()
                .Subscribe(_ => OnUpdateOwner())
                .AddTo(_owner);
        }

        private void UpdateStatus()
        {
            status.Update();

            if (_enableAttack)
                _renderController.SetFloat(AtkSpeedHash, status.atkSpeed);

            if (_enableMove)
                navAgent.maxSpeed = status.speed;
        }

        private void UpdateState()
        {
            if (_attack is { InProgress: true })
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

            _renderController.SetAnimation(hash);
        }

        private void UpdateLookDirection(Vector2 direction) => _look.At(navAgent.position, direction);

        private void OnPrepare()
        {
            _finding.Clear();

            state.Value = States.Idle;
        }

        private void OnIdle()
        {
            if (TrySetToAttack()) return;
            if (TrySetToMove()) return;
            if (TrySetToReturn()) return;
        }

        private bool TrySetToAttack()
        {
            if (!AllowAttack)
                return false;
            
            if (!_finding.IsAbleToAttack(navAgent.position))
                return false;

            if (state.Value == States.Attack)
                return true;

            state.Value = States.Attack;
            _lookDirection.Value = _finding.attack.Found[0].position;

            _attack.Execute();
            navAgent.Stop();
            return true;
        }
        
        private bool TrySetToMove()
        {
            if (!AllowMove)
                return false;
            
            var position = navAgent.position;
            if (!_finding.TryFindMoveTarget(position, out var target))
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
            _lookDirection.Value = _finding.move.Found[0].position;
            
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