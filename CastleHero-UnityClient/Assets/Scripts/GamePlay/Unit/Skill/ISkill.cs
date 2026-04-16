using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.GamePlay.Unit.Effects;
using CastleHero.GamePlay.Unit.Events;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.GamePlay.Unit.Skill.Components;
using CastleHero.Utility;
using CastleHero.GamePlay.InGame;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Skill
{
    /// <summary>
    /// 스킬 외부 계약: 읽기만 허용. 구성은 SkillBuilder (생성자/Activator + 프로퍼티 주입) 를 통해 이뤄진다.
    /// 구현체 SkillBase 는 내부 프로퍼티 set 을 public 으로 유지하여 빌더가 쓸 수 있도록 한다.
    /// </summary>
    public interface ISkill
    {
        UnitBehaviour Owner { get; }
        SkillEntity Data { get; }
        IBound Bound { get; }
        ITargeting Targeting { get; }
        ICycle Cycle { get; }
        IRunner Runner { get; }

        void Init();
        void SetToEnable();
    }

    public abstract class SkillBase : ISkill
    {   
        protected struct AroundCenter
        {
            public UnitBehaviour center;
            public IEnumerable<UnitBehaviour> units;
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
            var center = centerUnit.Position;
            var forward = (center - Owner.Position).normalized;
            var units = Bound.UnitsInBound(center, forward);
            int count = units.Count;
            int validCount = maxCount == 0 || maxCount > count ? count : maxCount;

            var castle = InGameSession.Current.Castle.Value;
            var around = units.Where(x =>
                {
                    bool filterA = includeCenter || x != centerUnit;
                    bool filterB = includeCastle || x != castle;
                    bool filterC = x.Type == UnitBehaviour.BehaviourType.Unit;

                    return filterA && filterB && filterC;
                })
                .Take(validCount);

            aroundCenter.center = centerUnit;
            aroundCenter.forward = forward;
            aroundCenter.units = around;
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

        protected void PublishRestriction(UnitBehaviour unit, UnitCore.Restrictions type, float duration, string effect = "")
        {
            new RestrictionEvent
            {
                From = Owner,
                To = unit,
                Type =  type,
                Duration = duration,
                Effect = effect,
            }.Publish();
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
            var list = InGameSession.Current.Characters;
            int length = list.Count;
            if (otherCondition == null)
                condition = (x) => x.IsValid();
            else
                condition = (x) => x.IsValid() && otherCondition.Invoke(x);

            return list
                .OfType<UnitBehaviour>()
                .Where(condition)
                .Take(maxCount == 0 || maxCount > length ? length : maxCount);
        }

        protected float GetAmount(Status.Type type, float multiplier) => Owner.Status[type] * multiplier;

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
    }
}