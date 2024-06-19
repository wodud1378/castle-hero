using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Effects;
using RGLabs.InGame.Effects.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Skill
{
    public class Skill10010 : ActiveSkill
    {
        private readonly List<IEffect> _effects = new();
        
        private IDisposable _subscription;
        private float _currentTime;
        
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity)  ||
                !TryUpdateAroundCenter(quantity))
                return;
            
            if (TryGetEffectPrefab(0, out var effect))
            {
                Effect.Builder
                    .StartBuild(effect)
                    .To(Owner)
                    .Run();
            }
            
            Attach().Forget();
        }

        private async UniTaskVoid Attach()
        {
            if (!TryUpdateAroundCenter())
                return;
            
            float duration = Data.duration;
            _currentTime = duration;
            _effects.Clear();

            if(TryGetEffectPrefab(1, out string effect))
            {
                var tasks = new List<UniTask<IEffect>>();
                foreach (var unit in aroundCenter.units)
                {
                    tasks.Add(Effect.Builder
                        .StartBuild(effect)
                        .To(unit)
                        .Duration(duration)
                        .RunAsync());
                }

                _effects.AddRange(await UniTask.WhenAll(tasks));
            }
            
            foreach (var unit in aroundCenter.units)
            {
                PublishRestriction(unit, UnitCore.Restrictions.Attack, duration);
                PublishRestriction(unit, UnitCore.Restrictions.Skill, duration);
                
                unit.Core.movement.Finder.RegisterOverride(Owner);
            }
            
            Owner.OnDead -= OnOwnerDead;
            Owner.OnDead += OnOwnerDead;

            _subscription = Owner
                .UpdateAsObservable()
                .Select(_ => Time.deltaTime)
                .Subscribe(Update)
                .AddTo(Owner);
        }

        private void Update(float deltaTime)
        {
            _currentTime -= deltaTime;
            if (_currentTime > 0f)
                return;

            Release();
        }

        private void Release()
        {
            foreach (var unit in aroundCenter.units)
            {
                if (!unit.IsValid())
                    continue;
                
                unit.Core.movement.Finder.ReleaseOverride(Owner);
            }
            
            _subscription.Dispose();

            foreach (var effect in _effects)
            {
                effect.Stop();
            }

            _effects.Clear();
        }

        private void OnOwnerDead(UnitBehaviour unit)
        {
            Release();
            
            unit.OnDead -= OnOwnerDead;
        }
    }
}