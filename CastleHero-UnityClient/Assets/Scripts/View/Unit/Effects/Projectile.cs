using CastleHero.Common.Behaviours;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Unit.Effects
{
    public class Projectile : PoolItemBase, IEffect
    {
        [FormerlySerializedAs("_speed")]
        [SerializeField] private float speed;
        [FormerlySerializedAs("_trail")]
        [SerializeField] private TrailRenderer trail;
        [FormerlySerializedAs("_particle")]
        [SerializeField] private ParticleSystem particle;

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

            if (trail != null)
                trail.enabled = true;

            if (particle != null)
                particle.Play(true);
        }

        public void SetTarget(UnitBehaviour unit)
        {
            _target = unit;
            _target.OnDead += OnTargetDead;

            SetTarget(_target.Position);
        }

        public void SetTarget(Vector2 position)
        {
            _destination = position;
            _arrivalTime = Vector2.Distance(transform.position, position) / speed;
        }

        public void Stop()
        {
            _isRunning = false;

            if (trail != null)
            {
                trail.Clear();
                trail.enabled = false;
            }

            if (particle != null)
                particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

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
                _destination = _target.Position;

            var diff = _destination - (Vector2)transform.position;
            if (diff.sqrMagnitude < 0.015f)
            {
                Stop();
                return;
            }

            float moveDistance = speed * Time.deltaTime;
            float distanceToTarget = diff.magnitude;

            // 목표 지점까지의 거리보다 더 많이 이동하지 않도록 보정
            float distanceToMove = Mathf.Min(moveDistance, distanceToTarget);

            var direction = diff.normalized;
            transform.localRotation = Quaternion.Euler(0, 0, 180f - direction.ToFloat());
            transform.Translate(direction * distanceToMove, Space.World);
        }

        private void OnValidate()
        {
            if (trail == null)
                trail = GetComponentInChildren<TrailRenderer>();

            if (particle == null)
                particle = GetComponentInChildren<ParticleSystem>();
        }
    }
}
