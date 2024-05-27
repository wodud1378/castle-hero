using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.InGame.Effects.Behaviours
{
    public class Effect : PoolItemBase, IEffect
    {
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

        public void Run()
        {
            _isRunning = true;
            _currentTime = duration;
            
            SetParticleActive(false);

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
        }

        public void SetTarget(UnitBehaviour unit)
        {
            if (unit.EffectBody == null)
                return;

            unit.EffectBody.Attach(this);
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
            DestroySelf();
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

        public static async void Play(string prefab, Vector2 position)
        {
            var effect = await GetEffect(prefab);
            if (effect == null)
                return;

            effect.SetTarget(position);
            effect.Run();
        }

        public static async void Play(string prefab, UnitBehaviour unit)
        {
            var effect = await GetEffect(prefab);
            if (effect == null)
                return;

            effect.SetTarget(unit);
            effect.Run();
        }

        private static async UniTask<IEffect> GetEffect(string prefab)
        {
            var container = Context.currentBehaviour.poolContainer;
            var pool = container.Get(prefab);
            return await pool.Get() as IEffect;
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