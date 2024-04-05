using System.Collections.Generic;
using RGLabs.InGame.Behaviours.Unit.Components;
using RGLabs.InGame.Common;
using RGLabs.InGame.Data.Model;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;

namespace RGLabs.InGame.Behaviours.Unit
{
    [RequireComponent(typeof(SortingGroup))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Movement))]
    public abstract class GameUnit : Obj
    {
        public enum States
        {
            None,
            Idle,
            Move,
            Attack,
            Dead,
        }

        public Vector2 Position
        {
            get => _rigidbody.position;
            set => _rigidbody.position = value;
        }

        private static readonly Dictionary<States, int> AnimationsHash = new()
        {
            { States.Idle, Animator.StringToHash("Idle") },
            { States.Move,  Animator.StringToHash("Move") },
            { States.Attack, Animator.StringToHash("Attack") },
            { States.Dead, Animator.StringToHash("Dead") },
        };

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Animator _animator;

        [SerializeField] protected Detecting _findingRange;
        [SerializeField] protected Detecting _attackRange;

        // Components
        [SerializeField] protected Movement _movement;
        [SerializeField] protected Attack[] _attackComponents;

        public ReactiveProperty<States> State { get; } = new(States.None);
        
        public void Init(UnitEntity data)
        {
            _movement.Root = this;
            _movement.Init(_rigidbody, _animator.transform, data.speed, data.range, _findingRange, Destination());
            foreach (var attack in _attackComponents)
            {
                attack.Root = this;
                attack.Init(_attackRange, data.atk);
            }

            State.Value = States.Idle;
        }

        private void Awake()
        {
            State
                .DistinctUntilChanged()
                .Subscribe(UpdateAnimation);
        }

        private void UpdateAnimation(States state)
        {
            if (state == States.None)
                return;

            _animator.SetTrigger(AnimationsHash[state]);
        }

        private void Update()
        {
            if (State.Value == States.Dead)
                return;

            if (_attackRange.HasDetected)
                State.Value = States.Attack;
            else if (_findingRange.HasDetected || _movement.MoveState.Value)
                State.Value = States.Move;
            else
                State.Value = States.Idle;
        }

        protected abstract Vector2 Destination();
    }
}