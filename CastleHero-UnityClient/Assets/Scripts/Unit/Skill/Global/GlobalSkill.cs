using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data.Model;
using RGLabs.InGame.Effects.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Skill.Components;
using RGLabs.Unit.Skill.Components.Factory;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace RGLabs.Unit.Skill.Global
{
    public class GlobalSkill
    {
        public readonly CastleSkillType type;
        public readonly float radius;
        public readonly CoolTime coolTime;

        private readonly string _centerEffect;
        private readonly string _unitEffect;
        private readonly string _sfx;
        private readonly float _value;

        private readonly UnitBehaviour _castle;
        private readonly CircleBound _bound;

        public GlobalSkill(UnitBehaviour castle, CastleSkillParameter parameter)
        {
            type = parameter.type;
            radius = parameter.radius;

            _value = parameter.value;
            _centerEffect = parameter.centerEffect;
            _unitEffect = parameter.unitEffect;
            _sfx = parameter.sfx;
            _castle = castle;

            _castle
                .UpdateAsObservable()
                .Subscribe()
                .AddTo(castle);

            _bound = new CircleBound(radius, radius);

            var targeting = type switch
            {
                CastleSkillType.Shield => Targeting.Alley,
                CastleSkillType.Sturn => Targeting.Enemy,
                CastleSkillType.Damage => Targeting.Enemy,
                CastleSkillType.Heal => Targeting.Alley,
                _ => default
            };

            _bound.Finder.detection.SetFilter(_castle, targeting);
            coolTime = new CoolTime(_castle, parameter.coolTime);
            coolTime.StartWaiting();
        }

        public void Execute(Vector2 position)
        {
            Action<UnitBehaviour> action = type switch
            {
                CastleSkillType.Shield => Shield,
                CastleSkillType.Damage => Damage,
                CastleSkillType.Sturn => Stun,
                CastleSkillType.Heal => Heal,
                _ => null
            };

            if (action == null)
                return;

            var units = _bound.UnitsInBound(position, default);
            units.ForEach(action);

            Context.sounds.PlaySfx(_sfx);
            Effect.Builder.Run(_centerEffect, position);
            
            coolTime.StartWaiting();
        }

        private void Shield(UnitBehaviour unit)
        {
            new ShieldEvent
            {
                From = _castle,
                To = unit,
                Amount = _value,
                Effect = _unitEffect,
                Duration = 0
            }.Publish();
        }

        private void Stun(UnitBehaviour unit)
        {
            new ShieldEvent
            {
                From = _castle,
                To = unit,
                Amount = _value,
                Effect = _unitEffect,
                Duration = 0
            }.Publish();
        }

        private void Damage(UnitBehaviour unit)
        {
            new AtkEvent
            {
                Type = DamageType.Normal,
                From = _castle,
                To = unit,
                Amount = _value,
                Effect = _unitEffect
            }.Publish();
        }

        private void Heal(UnitBehaviour unit)
        {
            new HealEvent
            {
                From = _castle,
                To = unit,
                Amount = _value,
                Effect = _unitEffect,
            }.Publish();
        }
    }
}