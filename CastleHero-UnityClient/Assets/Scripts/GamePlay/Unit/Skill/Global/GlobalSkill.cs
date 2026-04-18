using System;
using System.Linq;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.GamePlay.Unit.Events;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.GamePlay.Unit.Skill.Components;
using CastleHero.GamePlay.Unit.Skill.Components.Factory;
using CastleHero.Utility;
using UnityEngine;
using CastleHero.Common.Pattern;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.Common.Sound;

namespace CastleHero.GamePlay.Unit.Skill.Global
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

        private readonly UnitActor _castle;
        private readonly CircleBound _bound;
        private readonly int _targetId;

        public GlobalSkill(UnitActor castle, CastleSkillParameter parameter)
        {
            type = parameter.type;
            radius = parameter.radius;

            _value = parameter.value;
            _centerEffect = parameter.centerEffect;
            _unitEffect = parameter.unitEffect;
            _sfx = parameter.sfx;
            _castle = castle;

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
            Action<UnitActor> action = type switch
            {
                CastleSkillType.Shield => Shield,
                CastleSkillType.Damage => Damage,
                CastleSkillType.Sturn => Stun,
                CastleSkillType.Heal => Heal,
                _ => null
            };

            if (action == null)
                return;

            var units = _bound.UnitsInBound(position, default)
                .Where(x => x != _castle);

            foreach (var unit in units)
            {
                action.Invoke(unit);
            }
            
            _castle.SoundManager.PlaySfx(_sfx);
            _castle.EffectBuilder.Run(_centerEffect, position);
            
            coolTime.StartWaiting();
        }

        private void Shield(UnitActor unit)
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

        private void Stun(UnitActor unit)
        {
            new RestrictionEvent
            {
                From = _castle,
                To = unit,
                Type = CombatController.Restrictions.Attack,
                Effect = _unitEffect,
                Duration = _value
            }.Publish();
            
            new RestrictionEvent
            {
                From = _castle,
                To = unit,
                Type = CombatController.Restrictions.Move,
                Effect = _unitEffect,
                Duration = _value
            }.Publish();
            
            new RestrictionEvent
            {
                From = _castle,
                To = unit,
                Type = CombatController.Restrictions.Skill,
                Effect = _unitEffect,
                Duration = _value
            }.Publish();
        }

        private void Damage(UnitActor unit)
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

        private void Heal(UnitActor unit)
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