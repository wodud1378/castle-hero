using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Behaviours;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Effects
{
    public class EffectBuilder : IEffectBuilder
    {
        private readonly IPoolContainer _container;

        public EffectBuilder(IPoolContainer container)
        {
            _container = container;
        }

        private string _prefab;
        private Vector2 _from;
        private Vector2 _to;
        private UnitActor _target;
        private Vector2 _forward;
        private float _duration;

        public IEffectBuilder StartBuild(string prefab)
        {
            _prefab = prefab;

            Clear();
            return this;
        }

        public IEffectBuilder From(Vector2 position)
        {
            _from = position;
            return this;
        }

        public IEffectBuilder To(UnitActor unit)
        {
            _target = unit;
            return this;
        }

        public IEffectBuilder To(Vector2 position)
        {
            _target = null;
            _to = position;
            return this;
        }

        public IEffectBuilder LookAt(Vector2 forward)
        {
            _forward = forward;
            return this;
        }

        public IEffectBuilder Duration(float value)
        {
            _duration = value;
            return this;
        }

        public void Run(string prefab, Vector2 to)
        {
            StartBuild(prefab)
                .To(to)
                .Run();
        }

        public IEffect Run()
        {
            var effect = GetEffect(_prefab);
            if (effect == null)
                return null;

            if(_target == null)
                effect.SetTarget(_to);
            else
                effect.SetTarget(_target);

            if (_duration != 0f)
                effect.Duration = _duration;

            if(_forward != default)
                effect.SetForward(_forward);

            effect.Run(_from);
            return effect;
        }

        private void Clear()
        {
            _from = Vector2.zero;
            _to = Vector2.zero;
            _forward = Vector2.zero;
            _target = null;
            _duration = 0f;
        }

        private IEffect GetEffect(string prefab)
        {
            if (string.IsNullOrEmpty(prefab))
                return null;

            if (!_container.TryGet(prefab, out var item))
                return null;

            return item as IEffect;
        }
    }
}