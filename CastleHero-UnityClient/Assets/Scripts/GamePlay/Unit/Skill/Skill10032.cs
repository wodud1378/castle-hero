using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using CastleHero.Common.Pattern;

namespace CastleHero.GamePlay.Unit.Skill
{
    public class Skill10032 : ActiveSkill
    {
        private readonly List<IEffect> _effects = new();
        
        private IDisposable _subscription;
        private float _currentTime;
        
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity)  ||
                !TryUpdateAroundCenter(quantity))
                return;

            Attach().Forget();

            if (TryGetStatusParameter(1, out var type, out var value))
            {
                TryGetEffectPrefab(1, out string effect);
                PublishShield(Owner, GetAmount(type, value), 0f, effect);
            }
        }

        private async UniTaskVoid Attach()
        {
            if (!TryUpdateAroundCenter())
                return;
            
            float duration = Data.duration;
            _currentTime = duration;
            _effects.Clear();

            if(TryGetEffectPrefab(0, out string effect))
            {
                var tasks = new List<UniTask<IEffect>>();
                foreach (var unit in aroundCenter.units)
                {
                    tasks.Add(Owner.EffectBuilder
                        .StartBuild(effect)
                        .To(unit)
                        .Duration(duration)
                        .RunAsync());
                }

                _effects.AddRange(await UniTask.WhenAll(tasks));
            }
            
            foreach (var unit in aroundCenter.units)
            {
                PublishRestriction(unit, UnitCore.Restrictions.Skill, duration);
                
                unit.Core.Movement.Finder.RegisterOverride(Owner);
                unit.Core.Attack.finder.RegisterOverride(Owner);
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
                
                unit.Core.Movement.Finder.ReleaseOverride(Owner);
                unit.Core.Attack.finder.ReleaseOverride(Owner);
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