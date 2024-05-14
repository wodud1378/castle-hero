using RGLabs.Common.Behaviours;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Behaviours
{
    public class Projectile : PoolItemBase
    {
        [SerializeField] private float _speed;
        
        private UnitBehaviour _target;
        private Vector2 _destination;

        private float _arrivalTime;
        
        public void Fire(UnitBehaviour target)
        {
            if (!target.IsValid())
                return;
            
            _target = target;
            _arrivalTime = Vector2.Distance(transform.position, _target.position) / _speed;
            
            target.OnDead += OnTargetDead;
        }

        private void OnTargetDead(UnitBehaviour unit)
        {
            _target = null;

            unit.OnDead -= OnTargetDead;
        }

        private void Update()
        {
            _arrivalTime -= Time.deltaTime;
            if (_arrivalTime < 0f)
            {
                DestroySelf();
                return;
            }
            
            if (_target.IsValid())
                _destination = _target.position;

            var diff = _destination - (Vector2)transform.position;
            if (diff.sqrMagnitude < 0.015f)
            {
                DestroySelf();
                return;
            }
            
            var direction = diff.normalized;
            transform.localRotation = Quaternion.Euler(0, 0, 180f - direction.ToFloat());
            transform.Translate(direction * (_speed * Time.deltaTime), Space.World);
        }
    }
}