using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.InGame.Effects.Behaviours
{
    public class Effect : PoolItemBase, IEffect
    {
        public static readonly EffectBuilder Builder = new();
            
        public enum Slot
        {
            Top,
            Middle,
            Bottom,
        }

        [SerializeField] private Effect[] _children;
        [SerializeField] private ParticleSystem[] _particles;

        public Slot slot = Slot.Bottom;
        public float duration;

        private bool _isRunning;
        private float _currentTime;

        public float Duration
        {
            get => duration;
            set => duration = value;
        }

        public void SetForward(Vector2 forward) => transform.localRotation = Quaternion.Euler(0, 0, 180f - forward.ToFloat());

        public void Run(Vector2 _ = default)
        {
            _isRunning = true;
            _currentTime = duration;
            
            SetParticleActive(false);

            foreach (var particle in _particles)
            {
                particle.Play();
            }

            foreach (var child in _children)
            {
                child.duration = duration;
                child.Run();
            }
        }

        public void Stop()
        {
            SetParticleActive(false);

            foreach (var child in _children)
                child.Stop();
            
            DestroySelf();
        }

        public void SetTarget(UnitBehaviour unit)
        {
            var effectBody = unit.EffectBody;
            if (effectBody == null)
                return;

            effectBody.Attach(this);
            foreach (var child in _children)
            {
                effectBody.Attach(child);
            }
        }

        public void SetTarget(Vector2 position) => transform.position = position;

        private void Update()
        {
            if (!_isRunning)
                return;

            _currentTime -= Time.deltaTime;
            if (_currentTime > 0f)
                return;

            _isRunning = false;
            Stop();
        }

        private void SetParticleActive(bool isActive)
        {
            Action<ParticleSystem> onParticle;
            if (isActive)
                onParticle = (x) =>
                {
                    if (x.isEmitting)
                        return;
                    
                    x.Play(true);
                };
            else
                onParticle = (x) =>
                {
                    if (!x.isEmitting)
                        return;
                    
                    x.Stop(true);
                };

            foreach (var particle in _particles)
            {
                if (!particle.isEmitting)
                    continue;
                
                onParticle.Invoke(particle);
            }
        }
        
        private void OnValidate()
        {
            _children = GetComponentsInChildren<Effect>()
                .Where(x => x != this)
                .ToArray();

            _particles = GetComponentsInChildren<ParticleSystem>();
        }
    }
}