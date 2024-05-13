using RGLabs.Common.Behaviours;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Behaviours
{
    public class Projectile : PoolItemBase
    {
        [SerializeField] private float _speed;
        
        private UnitBehaviour _target;
        private Vector2 _destination;
        private bool _isDeadTarget;

        private float _arrivalTime;
        
        public void Fire(UnitBehaviour target)
        {
            if (!target.IsValid())
                return;
            
            _target = target;
            _isDeadTarget = false;
            _arrivalTime = Vector2.Distance(_target.Center, transform.position) / _speed;
            
            this
                .UpdateAsObservable()
                .RepeatUntilDisable(this)
                .Subscribe(_=> UpdatePosition());

            target.OnDead += OnTargetDead;
        }

        private void OnTargetDead(UnitBehaviour unit)
        {
            _isDeadTarget = true;

            unit.OnDead -= OnTargetDead;
        }

        private void UpdatePosition()
        {
            _arrivalTime -= Time.deltaTime;
            if (!_isDeadTarget)
                _destination = _target.Center;

            var diff = _destination - (Vector2)transform.position;
            if (diff.sqrMagnitude < 0.015f)
            {
                DestroySelf();
                return;
            }
            
            if (_arrivalTime < 0f)
            {
                DestroySelf();
                return;
            }
            
            var direction = diff.normalized;
            transform.localRotation = Quaternion.Euler(0, 0, 180f - direction.ToFloat());
            transform.Translate(direction * _speed * Time.deltaTime, Space.World);
        }
    }
}