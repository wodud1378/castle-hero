using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.InGame.Effects
{
    public class EffectBuilder
    {
        private string _prefab;
        private Vector2 _from;
        private Vector2 _to;
        private UnitBehaviour _target;
        private Vector2 _forward;
        private float _duration;
        
        public EffectBuilder StartBuild(string prefab)
        {
            _prefab = prefab;
            
            Clear();
            return this;
        }

        public EffectBuilder From(Vector2 position)
        {
            _from = position;
            return this;
        }

        public EffectBuilder To(UnitBehaviour unit)
        {
            _target = unit;
            return this;
        }
        
        public EffectBuilder To(Vector2 position)
        {
            _target = null;
            _to = position;
            return this;
        }

        public EffectBuilder LookAt(Vector2 forward)
        {
            _forward = forward;
            return this;
        }

        public EffectBuilder Duration(float value)
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

        public async void Run()
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
        
        private async UniTask<IEffect> GetEffect(string prefab)
        {
            if (string.IsNullOrEmpty(prefab))
                return null;
            
            var container = Context.poolContainer;
            var item = await container.GetItem(prefab);
            if (item == null)
                return null;

            if (item is IEffect effect)
                return effect;
            
            return null;
        }
    }
}