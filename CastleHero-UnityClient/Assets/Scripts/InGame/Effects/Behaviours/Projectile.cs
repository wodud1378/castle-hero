using RGLabs.Common.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace RGLabs.InGame.Effects.Behaviours
{
    public class Projectile : PoolItemBase, IEffect
    {
        [SerializeField] private float _speed;

        private UnitBehaviour _target;
        private Vector2 _destination;

        private bool _isRunning = false;
        private float _arrivalTime;

        public float Duration { get; set; }

        public void SetForward(Vector2 forward) { }

        public void Run(Vector2 startAt = default)
        {
            transform.position = startAt;
            _isRunning = true;
        }

        public void SetTarget(UnitBehaviour unit)
        {
            _target = unit;
            _target.OnDead += OnTargetDead;

            SetTarget(_target.position);
        }

        public void SetTarget(Vector2 position)
        {
            _destination = position;
            _arrivalTime = Vector2.Distance(transform.position, position) / _speed;
        }

        public void Stop()
        {
            _isRunning = false;
            DestroySelf();
        }

        private void OnTargetDead(UnitBehaviour unit)
        {
            _target = null;

            unit.OnDead -= OnTargetDead;
        }

        private void Update()
        {
            if (!_isRunning)
                return;

            _arrivalTime -= Time.deltaTime;
            if (_arrivalTime < 0f)
            {
                Stop();
                return;
            }

            if (_target.IsValid())
                _destination = _target.position;

            var diff = _destination - (Vector2)transform.position;
            if (diff.sqrMagnitude < 0.015f)
            {
                Stop();
                return;
            }

            var direction = diff.normalized;
            transform.localRotation = Quaternion.Euler(0, 0, 180f - direction.ToFloat());
            transform.Translate(direction * (_speed * Time.deltaTime), Space.World);
        }
    }
}