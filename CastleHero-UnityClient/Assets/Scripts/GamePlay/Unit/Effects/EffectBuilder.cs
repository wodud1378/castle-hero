using Cysharp.Threading.Tasks;
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
        private UnitBehaviour _target;
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

        public IEffectBuilder To(UnitBehaviour unit)
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
                .RunAsync()
                .Forget();
        }

        public async UniTask Run()
        {
            var effect = await GetEffect(_prefab);
            if (effect == null)
                return;
            
            if(_target == null)
                effect.SetTarget(_to);
            else
                effect.SetTarget(_target);

            if (_duration != 0f)
                effect.Duration = _duration;
            
            if(_forward != default)
                effect.SetForward(_forward);
            
            effect.Run(_from);
        }

        public async UniTask<IEffect> RunAsync()
        {
            var effect = await GetEffect(_prefab);
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
        
        private UniTask<IEffect> GetEffect(string prefab)
        {
            if (string.IsNullOrEmpty(prefab))
                return UniTask.FromResult<IEffect>(null);

            if (!_container.TryGet(prefab, out var item))
                return UniTask.FromResult<IEffect>(null);

            return UniTask.FromResult(item as IEffect);
        }
    }
}