using System;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Skill
{
    public class Skill10019 : ActiveSkill
    {
        private IDisposable _heal;
        private int _count;
        private int _currentCount;
        private float _currentTime;
        
        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity) ||
                !TryUpdateAroundCenter(quantity, true))
                return;

            AttachHeal();
        }
        
        private void AttachHeal()
        {
            _count = Mathf.CeilToInt(Data.duration) / 1;
            _currentCount = 0;

            _heal = Owner
                .UpdateAsObservable()
                .Select(_ => Time.deltaTime)
                .Subscribe(Heal)
                .AddTo(Owner);
        }
        
        private void Heal(float deltaTime)
        {
            _currentTime -= deltaTime;
            if (_currentTime > 0f)
                return;

            _currentTime = 1f;
            if (!TryGetStatusParameter(0, out var type, out var value))
                return;

            TryGetEffectPrefab(0, out string eff);
            
            float amount = GetAmount(type, value);
            foreach (var target in aroundCenter.units)
            {
                if (!target.IsValid())
                    continue;
                
                PublishHeal(target, amount, eff);
            }
            
            if (++_currentCount < _count)
                return;

            _heal.Dispose();
        }
    }
}