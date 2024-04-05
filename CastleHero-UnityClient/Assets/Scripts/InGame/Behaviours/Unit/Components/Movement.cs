using RGLabs.InGame.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Unit.Components
{
    public class Movement : MonoBehaviour, IUnitComponent
    {
        public enum Direction
        {
            Left,
            Right,
        }

        public GameUnit Root { get; set; }
        public ReactiveProperty<bool> MoveState { get; } = new(false);

        private Rigidbody2D _rigidbody;
        private Transform _directionRoot;
        private Vector3 _originScale;
        private Vector3 _scaleByDirection;

        private float _speed;
        private float _threshold;

        private Vector2 _originDestination;
        private Vector2 _destination;
        private Vector2 _directionVector;

        private Detecting _detector;

        private Direction _direction;

        public void Init(Rigidbody2D rigidbody, Transform directionRoot, float speed, float threshold,
            Detecting detector,
            Vector2 destination)
        {
            _rigidbody = rigidbody;
            _directionRoot = directionRoot;
            _originScale = directionRoot.localScale;
            _speed = speed;
            _threshold = threshold;
            _detector = detector;
            _originDestination = destination;
        }

        private void Update()
        {
            UpdateDirection();
            LookDirection();
        }

        private void UpdateDirection()
        {
            var target = _detector[0];
            _destination = target.IsValid() ? target.Position : _originDestination;

            var diff = _destination - Root.Position;
            _directionVector = diff.normalized;
            _direction = diff.x <= 0 ? Direction.Left : Direction.Right;
        }

        private void LookDirection()
        {
            float xScale = _direction == Direction.Left ? _originScale.x : -_originScale.x;
            _directionRoot.localScale = new Vector3(xScale, _originScale.y, _originScale.z);
        }

        private void FixedUpdate()
        {
            var moveAmount = _directionVector * (_speed * Time.fixedDeltaTime);
            var pos = _rigidbody.position;
            if (Vector2.Distance(pos, _destination) < _threshold)
            {
                MoveState.Value = false;
                return;
            }

            MoveState.Value = true;
            _rigidbody.MovePosition(pos + moveAmount);
        }
    }
}