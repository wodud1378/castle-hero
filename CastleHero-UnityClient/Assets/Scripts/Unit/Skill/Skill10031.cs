using System;
using System.Collections.Generic;
using RGLabs.InGame.Effects.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Skill
{
    public class Skill10031 : ActiveSkill
    {
        private enum Step
        {
            Attack = 0,
            DotDamage,
            Shield,
        }

        private readonly List<UnitBehaviour> _dotDamageTargets = new();
        private IDisposable _dotDamage;
        
        private int _count;
        private int _currentCount;
        private float _currentTime;
        private float _dotDamageAmount;
        
        protected override void OnExecute()
        {
            ShieldOnAlley();
            AttackEnemies();
        }

        private void ShieldOnAlley()
        {
            Bound.Finder.detection.Filter = Owner.Core.alleyLayerMask;
            if (!TryUpdateAroundCenter() ||
                !TryGetStatusParameter(Step.Shield, out var type, out var value))
                return;

            TryGetEffectPrefab(Step.Shield, out var effect);
            
            var amount = GetAmount(type, value);
            foreach (var unit in aroundCenter.units)
            {
                PublishShield(unit, amount, 0f, effect);
            }
            
            AttachDotDamage();
        }

        private void AttackEnemies()
        {
            Bound.Finder.detection.Filter = Owner.Core.alleyLayerMask;
            if (!TryGetQuantityParameter(Step.Attack, out int quantity) ||
                !TryUpdateAroundCenter(quantity) ||
                !TryGetStatusParameter(Step.Attack, out var type, out var value))
                return;
            
            if (TryGetEffectPrefab(Step.Attack, out var effect))
            {
                Effect.Builder
                    .StartBuild(effect)
                    .To(aroundCenter.center.position)
                    .Run();
            }

            float amount = GetAmount(type, value);
            float duration = Data.duration;
            foreach (var unit in aroundCenter.units)
            {
                PublishAtk(unit, DamageType.Normal, amount);
                
                if (TryGetEffectPrefab(Step.DotDamage, out  effect))
                {
                    Effect.Builder
                        .StartBuild(effect)
                        .To(unit)
                        .Duration(duration)
                        .Run();
                }
            }
        }

        private void AttachDotDamage()
        {
            if (!TryGetStatusParameter(Step.DotDamage, out var type, out var value))
                return;
            
            _count = Mathf.CeilToInt(Data.duration) / 1;
            _currentCount = 0;
            _dotDamageAmount = GetAmount(type, value);
            
            _dotDamageTargets.Clear();
            _dotDamageTargets.AddRange(aroundCenter.units);

            _dotDamage = Owner
                .UpdateAsObservable()
                .Select(_ => Time.deltaTime)
                .Subscribe(DotDamage)
                .AddTo(Owner);
        }

        private void DotDamage(float deltaTime)
        {
            _dotDamageTargets.RemoveAll(x => !x.IsValid());
            
            _currentTime -= deltaTime;
            if (_currentTime > 0f)
                return;

            _currentTime = 1f;

            TryGetEffectPrefab(Step.DotDamage, out string eff);
            foreach (var target in _dotDamageTargets)
            {
                PublishAtk(target, DamageType.Debuff, _dotDamageAmount, eff);
            }
            
            if (++_currentCount < _count)
                return;
            
            _dotDamage.Dispose();
        }
    }
}