using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
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

        protected void PublishAtk(UnitBehaviour unit, DamageType type, float amount)
        {
            new AtkEvent
            {
                Type = type,
                From = Owner,
                To = unit,
                Amount = amount,
            }.Publish();
        }

        protected void PublishHeal(UnitBehaviour unit, float amount)
        {
            new HealEvent
            {
                From = Owner,
                To = unit,
                Amount = amount
            }.Publish();
        }

        protected void PublishShield(UnitBehaviour unit, float amount, float duration = 0f)
        {
            new ShieldEvent
            {
                From = Owner,
                To = unit,
                Amount = amount,
                Duration = duration
            }.Publish();
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
    }
}