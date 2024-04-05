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
            { States.Idle, Constants.IdleAnim },
            { States.Move, Constants.MoveAnim },
            { States.Attack, Constants.AtkAnim },
            { States.Dead, Constants.DeadAnim },
        };

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Animator _animator;

        [SerializeField] protected Detecting _findingRange;
        [SerializeField] protected Detecting _attackRange;

        // Components
        [SerializeField] protected Movement _movement;
        [SerializeField] protected Attack[] _attackComponents;

        public ReactiveProperty<States> State { get; } = new(States.Idle);

        public void Init(UnitEntity data)
        {
            _movement.Root = this;
            _movement.Init(_rigidbody, _animator.transform, data.speed, _findingRange, Destination());
            foreach (var attack in _attackComponents)
            {
                attack.Root = this;
                attack.Init(_animator, _attackRange, data.atk);
            }
        }

        private void Awake()
        {
            State
                .DistinctUntilChanged()
                .Subscribe(x => _animator.SetTrigger(AnimationsHash[x]));
        }

        private void Update()
        {
            if (State.Value == States.Dead)
                return;

            if (_attackRange.HasDetected)
                State.Value = States.Attack;
            else if (_findingRange.HasDetected)
                State.Value = States.Move;
            else
                State.Value = States.Idle;
        }

        protected abstract Vector2 Destination();
    }
}