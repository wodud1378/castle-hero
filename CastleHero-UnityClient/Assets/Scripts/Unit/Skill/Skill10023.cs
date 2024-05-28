using System;
using System.Linq;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Skill
{
    public class Skill10023 : ActiveSkill
    {
        private enum Step
        {
            Heal = 0,
            Shield
        }

        private enum Effect
        {
            Self,
            HealTarget,
            ShieldTarget,
        }

        private IDisposable _heal;
        private int _count;
        private int _currentCount;
        private float _currentTime;

        protected override void OnExecute()
        {
            if (!TryGetQuantityParameter(0, out int quantity) ||
                !TryUpdateAroundCenter(quantity))
                return;
            
            var center = aroundCenter.center;
            PlayEffect(Effect.Self, center);

            AttachHeal();
            ShieldOnGroup();
        }

        private void ShieldOnGroup()
        {
            if (!TryGetStatusParameter(Step.Shield, out var type, out var value))
                return;

            if (!TryGetGroupParameter(0, out int group))
                return;

            TryGetEffectPrefab(Effect.ShieldTarget, out string eff);
            
            var targets = aroundCenter.around.Where(x => x.Data.team == group);
            float amount = GetAmount(type, value);
            foreach (var target in targets)
            {
                PublishShield(target, amount, 0f, eff);
            }
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
            if (!TryGetStatusParameter(Step.Heal, out var type, out var value))
                return;

            TryGetEffectPrefab(Effect.HealTarget, out string eff);
            
            float amount = GetAmount(type, value);
            foreach (var target in aroundCenter.around)
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