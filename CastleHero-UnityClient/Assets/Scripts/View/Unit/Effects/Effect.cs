using System;
using System.Linq;
using CastleHero.Common.Behaviours;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Unit.Effects
{
    public class Effect : PoolItemBase, IEffect
    {
        public enum Slot
        {
            Top,
            Middle,
            Bottom,
        }

        [FormerlySerializedAs("_children")]
        [SerializeField] private Effect[] children;
        [FormerlySerializedAs("_particles")]
        [SerializeField] private ParticleSystem[] particles;

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

            foreach (var particle in particles)
            {
                particle.Play();
            }

            foreach (var child in children)
            {
                child.duration = duration;
                child.Run();
            }
        }

        public void Stop()
        {
            SetParticleActive(false);

            foreach (var child in children)
                child.Stop();

            DestroySelf();
        }

        public void SetTarget(UnitBehaviour unit)
        {
            var effectBody = unit.EffectBody;
            if (effectBody == null)
                return;

            effectBody.Attach(this);
            foreach (var child in children)
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

            foreach (var particle in particles)
            {
                if (!particle.isEmitting)
                    continue;

                onParticle.Invoke(particle);
            }
        }

        private void OnValidate()
        {
            children = GetComponentsInChildren<Effect>()
                .Where(x => x != this)
                .ToArray();

            particles = GetComponentsInChildren<ParticleSystem>();
        }
    }
}
