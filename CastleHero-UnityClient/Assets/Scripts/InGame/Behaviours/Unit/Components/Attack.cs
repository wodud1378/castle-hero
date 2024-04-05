using System.Collections.Generic;
using RGLabs.InGame.Behaviours.Unit.Animation;
using RGLabs.InGame.Common;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Unit.Components
{
    public class Attack : MonoBehaviour, IUnitComponent
    {
        [SerializeField] private AttackAnimationEvents _events;

        public GameUnit Root { get; set; }
        
        private Animator _animator;
        private Detecting _detector;
        private float _damage;

        public void Init(Animator animator, Detecting detector, float damage)
        {
            _animator = animator;
            _damage = damage;

            _events.OnHitEvent -= OnHit;
            _events.OnHitEvent += OnHit;
        }
        
        public void Enable()
        {
        }

        public void Disable()
        {
        }
        
        private void OnHit()
        {
            foreach (var target in _detector.Targets)
            {
                // TODO 공격 로직
            }
        }

        private void OnValidate()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();
        }
    }
}