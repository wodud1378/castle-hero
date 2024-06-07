using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Common;
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

        public void SetToEnable();
    }

    public abstract class SkillBase : ISkill
    {
        protected struct AroundCenter
        {
            public UnitBehaviour center;
            public IEnumerable<UnitBehaviour> around;
            public Vector2 forward;
        }

        public UnitBehaviour Owner { get; set; }
        public SkillEntity Data { get; set; }
        public IBound Bound { get; set; }
        public ITargeting Targeting { get; set; }
        public ICycle Cycle { get; set; }
        public IRunner Runner { get; set; }
        public abstract void Init();

        protected AroundCenter aroundCenter;

        public void SetToEnable()
        {
            Cycle.StartWaiting();
        }

        protected bool TryUpdateAroundCenter(int maxCount = 0, bool includeCenter = false, bool includeCastle = false)
        {
            if (!Targeting.HasTargets())
                return false;

            var centerUnit = Targeting.Targets[0];
            var center = centerUnit.position;
            var forward = (center - Owner.position).normalized;
            var units = Bound.UnitsInBound(center, forward);
            int count = units.Count;
            int validCount = maxCount == 0 || maxCount > count ? count : maxCount;

            var castle = Storage.inGameRepository.castle.Value;
            var around = units.Where(x =>
                {
                    bool filterA = includeCenter || x != centerUnit;
                    bool filterB = includeCastle || x != castle;

                    return filterA && filterB;
                })
                .Take(validCount);

            aroundCenter.center = centerUnit;
            aroundCenter.forward = forward;
            aroundCenter.around = around;
            return true;
        }

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

        protected IEnumerable<UnitBehaviour> Characters(Func<UnitBehaviour, bool> otherCondition = null,
            int maxCount = 0)
        {
            Func<UnitBehaviour, bool> condition;
            var list = Storage.inGameRepository.characters;
            int length = list.Count;
            if (otherCondition == null)
                condition = (x) => x.IsValid();
            else
                condition = (x) => x.IsValid() && otherCondition.Invoke(x);

            return list
                .Where(condition)
                .Take(maxCount == 0 || maxCount > length ? length : maxCount);
        }

        protected float GetAmount(Status.Type type, float multiplier) => Owner.status[type] * multiplier;

        private bool TryLoad<T>(T[] array, int index, out T result)
        {
            if (!index.IsValidIndex(array))
            {
                result = default;
                return false;
            }

            result = array[index];
            return true;
        }

        protected bool TryGetQuantityParameter<T>(T index, out int quantity) where T : unmanaged, Enum
        {
            if (TryGetQuantityParameter(index.CastToInt(), out quantity))
            {
                quantity = quantity == 0 ? Constants.BufferSize : quantity;
                return true;
            }

            return false;
        }

        protected bool TryGetGroupParameter<T>(T index, out int group) where T : unmanaged, Enum
            => TryGetGroupParameter(index.CastToInt(), out group);

        protected bool TryGetStatusParameter<T>(T index, out Status.Type type, out float value) where T : unmanaged, Enum
            => TryGetStatusParameter(index.CastToInt(), out type, out value);

        protected bool TryGetEffectPrefab<T>(T index, out string prefab) where T : unmanaged, Enum
            => TryGetEffectPrefab(index.CastToInt(), out prefab);

        protected bool TryGetQuantityParameter(int index, out int quantity)
        {
            if (TryLoad(Data.targetQty, index, out quantity) && quantity != -1)
                return true;

            return false;
        }

        protected bool TryGetGroupParameter(int index, out int group)
        {
            if (TryLoad(Data.groups, index, out group) && group != -1)
                return true;

            return false;
        }

        protected bool TryGetStatusParameter(int index, out Status.Type type, out float value)
        {
            type = default;
            value = default;

            if (TryLoad(Data.stats, index, out var stat) && stat != -1)
                type = (Status.Type)stat;
            else
                return false;

            return TryLoad(Data.values, index, out value);
        }

        protected bool TryGetEffectPrefab(int index, out string prefab)
        {
            if (TryLoad(Data.effects, index, out prefab) && !string.IsNullOrEmpty(prefab))
                return true;

            return false;
        }

        protected void PlayEffect<T>(T index, Vector2 position) where T : Enum
            => PlayEffect(Convert.ToInt32(index), position);

        protected void PlayEffect<T>(T index, UnitBehaviour target) where T : Enum
            => PlayEffect(Convert.ToInt32(index), target);

        protected void PlayEffect(int index, Vector2 position)
        {
            if (!index.IsValidIndex(Data.effects))
                return;

            Effect.Play(Data.effects[index], position);
        }

        protected void PlayEffect(int index, UnitBehaviour target)
        {
            if (!index.IsValidIndex(Data.effects))
                return;

            Effect.Play(Data.effects[index], target);
        }
    }
}