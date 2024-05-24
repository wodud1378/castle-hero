using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.InGame.Effects.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Skill.Components;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Skill
{
    public interface ISkill
    {
        public UnitBehaviour Owner { get; set; }
        public SkillEntity Data { get; set; }
        public IBound Bound { get; set; }
        public ITargeting Targeting { get; set; }
        public ICycle Cycle { get; set; }
        public IRunner Runner { get; set; }

        public void Init();
    }

    public abstract class SkillBase : ISkill
    {
        public UnitBehaviour Owner { get; set; }
        public SkillEntity Data { get; set; }
        public IBound Bound { get; set; }
        public ITargeting Targeting { get; set; }
        public ICycle Cycle { get; set; }
        public IRunner Runner { get; set; }
        public abstract void Init();

        protected void PublishAtk(UnitBehaviour unit, DamageType type, float amount, string effect = "")
        {
            new AtkEvent
            {
                Type = type,
                From = Owner,
                To = unit,
                Amount = amount,
                Effect = effect,
            }.Publish();
        }

        protected void PublishHeal(UnitBehaviour unit, float amount, string effect = "")
        {
            new HealEvent
            {
                From = Owner,
                To = unit,
                Amount = amount,
                Effect = effect
            }.Publish();
        }

        protected void PublishShield(UnitBehaviour unit, float amount, float duration, string effect = "")
        {
            new ShieldEvent
            {
                From = Owner,
                To = unit,
                Amount = amount,
                Duration = duration,
                Effect = effect,
            }.Publish();
        }

        protected void PublishBuff(UnitBehaviour unit, Status.Type type, float amount, float duration,
            bool isMultiplier, string effect = "")
        {
            GetStatusEffectEvent(unit, type, amount, duration, true, isMultiplier, effect).Publish();
        }

        protected void PublishDebuff(UnitBehaviour unit, Status.Type type, float amount, float duration,
            bool isMultiplier, string effect = "")
        {
            GetStatusEffectEvent(unit, type, amount, duration, false, isMultiplier, effect).Publish();
        }

        private StatusEffectEvent GetStatusEffectEvent(UnitBehaviour unit, Status.Type type,
            float amount, float duration, bool isIncrease, bool isMultiplier, string effect)
        {
            return new StatusEffectEvent
            {
                From = Owner,
                To = unit,
                Type = type,
                IsMultiplier = isMultiplier,
                IsIncrease = isIncrease,
                Duration = duration,
                Amount = amount,
                Effect = effect
            };
        }

        protected IEnumerable<UnitBehaviour> Characters(Func<UnitBehaviour, bool> otherCondition = null)
        {
            Func<UnitBehaviour, bool> condition;

            if (otherCondition == null)
                condition = (x) => x.IsValid();
            else
                condition = (x) => x.IsValid() && otherCondition.Invoke(x);

            return Storage.inGameRepository.characters.Value
                .Where(condition);
        }

        protected float WithOwner(Status.Type type, float multiplier) => Owner.status[type] * multiplier;

        protected bool TryGetGroupParameter<T>(T index, out int group) where T : Enum
            => TryGetGroupParameter(Convert.ToInt32(index), out group);

        protected bool TryGetGroupParameter(int index, out int group)
        {
            if (!index.IsValidIndex(Data.groups))
            {
                group = 0;
                return false;
            }

            group = Data.groups[index];
            return true;
        }

        protected bool TryGetStatusParameter<T>(T index, out Status.Type type, out float value) where T : Enum
            => TryGetStatusParameter(Convert.ToInt32(index), out type, out value);

        protected bool TryGetStatusParameter(int index, out Status.Type type, out float value)
        {
            if (!index.IsValidIndex(Data.stats, Data.values))
            {
                type = default;
                value = 0f;
                return false;
            }

            type = (Status.Type)Data.stats[index];
            value = Data.stats[index];
            return true;
        }

        protected bool TryGetEffectPrefab<T>(T index, out string prefab) where T : Enum
            => TryGetEffectPrefab(Convert.ToInt32(index), out prefab);

        protected bool TryGetEffectPrefab(int index, out string prefab)
        {
            if (!index.IsValidIndex(Data.effects))
            {
                prefab = string.Empty;
                return false;
            }

            prefab = Data.effects[index];
            return true;
        }

        protected void PlayEffect<T>(T index, Vector2 position) where T : Enum
            => PlayEffect(Convert.ToInt32(index), position);

        protected void PlayEffect(int index, Vector2 position)
        {
            if (!index.IsValidIndex(Data.effects))
                return;

            Effect.Play(Data.effects[index], position);
        }

        protected void PlayEffect<T>(T index, UnitBehaviour target) where T : Enum
            => PlayEffect(Convert.ToInt32(index), target);

        protected void PlayEffect(int index, UnitBehaviour target)
        {
            if (!index.IsValidIndex(Data.effects))
                return;

            Effect.Play(Data.effects[index], target);
        }
    }
}