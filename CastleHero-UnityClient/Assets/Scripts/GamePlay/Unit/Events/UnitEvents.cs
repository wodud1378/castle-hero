using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using UnityEngine;

namespace CastleHero.GamePlay.Unit.Events
{
    public enum DamageType
    {
        Normal,
        Debuff
    }

    public struct UnitDead
    {
        public UnitBehaviour unit;
    }

    public interface IUnitEvent
    {
        UnitBehaviour From { get; }
        UnitBehaviour To { get; }
        float Amount { get; }
        string Effect { get; }
    }

    public interface IUnitEventResult
    {
        IUnitEvent Event { get; }
    }

    public struct AtkEvent : IUnitEvent
    {
        public DamageType Type { get; set; }
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public float Amount { get; set; }
        public string Effect { get; set; }
    }

    public struct HealEvent : IUnitEvent
    {
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public float Amount { get; set; }
        public string Effect { get; set; }
    }

    public struct ShieldEvent : IUnitEvent
    {
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public float Amount { get; set; }
        public string Effect { get; set; }
        public float Duration { get; set; }
    }

    public struct RestrictionEvent : IUnitEvent
    {
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public UnitCore.Restrictions Type { get; set; }
        public float Amount { get; set; }
        public string Effect { get; set; }
        public float Duration { get; set; }
    }

    public struct StatusEffectEvent : IUnitEvent
    {
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public Status.Type Type { get; set; }
        public bool IsMultiplier { get; set; }
        public bool IsIncrease { get; set; }
        public float Duration { get; set; }
        public float Amount { get; set; }
        public string Effect { get; set; }
    }

    public struct AtkResult : IUnitEventResult
    {
        public IUnitEvent Event { get; set; }
        public bool IsCritical { get; set; }
        public float Protected { get; set; }
    }

    public struct HealResult : IUnitEventResult
    {
        public IUnitEvent Event { get; set; }
    }

    public struct ShieldResult : IUnitEventResult
    {
        public IUnitEvent Event { get; set; }
    }
}
