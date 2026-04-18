using System;
using System.Collections.Generic;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using CastleHero.Common.Pattern;
using CastleHero.Utility;

namespace CastleHero.GamePlay.Unit.Skill
{
    public class Skill10032 : ActiveSkill
    {
        private readonly List<IEffect> _effects = new();

        private IDisposable _subscription;
        private IDisposable _ownerDeadSub;
        private float _currentTime;
        
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity)  ||
                !TryUpdateAroundCenter(quantity))
                return;

            Attach();

            if (TryGetStatusParameter(1, out var type, out var value))
            {
                TryGetEffectPrefab(1, out string effect);
                PublishShield(Owner, GetAmount(type, value), 0f, effect);
            }
        }

        private void Attach()
        {
            if (!TryUpdateAroundCenter())
                return;

            float duration = Data.duration;
            _currentTime = duration;
            _effects.Clear();

            if(TryGetEffectPrefab(0, out string effect))
            {
                foreach (var unit in aroundCenter.units)
                {
                    var e = Owner.EffectBuilder
                        .StartBuild(effect)
                        .To(unit)
                        .Duration(duration)
                        .Run();

                    if (e != null)
                        _effects.Add(e);
                }
            }

            foreach (var unit in aroundCenter.units)
            {
                PublishRestriction(unit, CombatController.Restrictions.Skill, duration);

                unit.Combat.Movement.Finder.RegisterOverride(Owner);
                unit.Combat.Attack.finder.RegisterOverride(Owner);
            }

            _ownerDeadSub?.Dispose();
            _ownerDeadSub = Owner.OnDead.Take(1).Subscribe(OnOwnerDead);

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
                
                unit.Combat.Movement.Finder.ReleaseOverride(Owner);
                unit.Combat.Attack.finder.ReleaseOverride(Owner);
            }
            
            _subscription.Dispose();

            foreach (var effect in _effects)
            {
                effect.Stop();
            }

            _effects.Clear();
        }

        private void OnOwnerDead(UnitActor unit)
        {
            Release();
        }
    }
}