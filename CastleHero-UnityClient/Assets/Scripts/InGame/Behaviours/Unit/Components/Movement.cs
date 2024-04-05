using RGLabs.Common.Pattern;
using RGLabs.InGame.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace RGLabs.InGame.Behaviours.Unit.Components
{
    public class Movement : MonoBehaviour, IUnitComponent
    {
        public GameUnit Root { get; set; }
        public ReactiveProperty<bool> MoveState { get; } = new(false);

        private Rigidbody2D _rigidbody;
        private Transform _directionRoot;
        private Vector3 _originScale;
        private Vector3 _scaleByDirection;

        private float _threshold;
        private float _speed;

        private Vector2 _originDestination;
        private Vector2 _destination;
        private Vector2 _direction;

        private Detecting _detector;

        public void Init(Rigidbody2D rigidbody, Transform directionRoot, float speed, Detecting detector,
            Vector2 destination)
        {
            _rigidbody = rigidbody;
            _directionRoot = directionRoot;
            _originScale = directionRoot.localScale;
            _speed = speed;
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
            var target = _detector.Targets[0];
            if (target.IsValid())
                _destination = target.Position;
            else
                _destination = _originDestination;

            _direction = (_destination - Root.Position).normalized;
        }

        private void LookDirection()
        {
            float xScale = _direction.x > 1 ? -_originScale.x : _originScale.x;
            _scaleByDirection.Set(xScale, _originScale.y, 1f);
            _directionRoot.localScale = _scaleByDirection;
        }

        private void FixedUpdate()
        {
            var moveAmount = _direction * (_speed * Time.fixedDeltaTime);
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