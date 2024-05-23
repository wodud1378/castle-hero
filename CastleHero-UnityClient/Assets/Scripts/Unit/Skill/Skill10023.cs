using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Skill
{
    public class Skill10023 : ActiveSkill
    {
        public enum Parameter
        {
            Heal = 0,
            Shield
        }
        
        private List<UnitBehaviour> _targets;
        private IDisposable _heal;
        private int _count;
        private int _currentCount;
        private float _currentTime;
        
        protected override void OnExecute()
        {
            var center = Targeting.Targets[0];
            _targets = Bound.FindTargets(center.position, Data.range, default);
            
            AttachHeal();
            ShieldOnGroup();
        }

        private void ShieldOnGroup()
        {
            if (!TryGetStatusParameter(Parameter.Shield, out var type, out var value))
                return;

            if (!TryGetGroupParameter(0, out int group))
                return;
            
            var targets = _targets.Where(x => x.Data.team == group);
            float amount = WithOwner(type, value); 
            foreach (var target in targets)
            {
                PublishShield(target, amount);
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

            _targets.RemoveAll(x => x.IsValid());
            _currentTime = 1f;
            
            if (!TryGetStatusParameter(Parameter.Heal, out var type, out var value))
                return;

            float amount = WithOwner(type, value); 
            foreach (var target in _targets)
            {
                PublishHeal(target, amount);
            }

            ++_currentCount;

            if (_currentCount < _count)
                return;
            
            _heal.Dispose();
        }
    }
}