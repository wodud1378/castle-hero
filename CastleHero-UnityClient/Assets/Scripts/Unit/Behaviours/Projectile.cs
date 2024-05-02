using RGLabs.Common.Behaviours;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Behaviours
{
    public class Projectile : PoolItemBase
    {
        private UnitBehaviour _target;
        private Vector2 _destination;
        private float _speed;
        private bool _isDeadTarget;
        
        public void Fire(UnitBehaviour target, float speed)
        {
            if (!target.IsValid())
                return;
            
            _target = target;
            _speed = speed;
            _isDeadTarget = false;
            
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
            if (!_isDeadTarget)
                _destination = _target.Center;

            var diff = _destination - (Vector2)transform.position;
            if (diff.sqrMagnitude < 0.01f)
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