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
        public UnitActor unit;
    }

    public interface IUnitEvent
    {
        UnitActor From { get; }
        UnitActor To { get; }
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
        public UnitActor From { get; set; }
        public UnitActor To { get; set; }
        public float Amount { get; set; }
        public string Effect { get; set; }
    }

    public struct HealEvent : IUnitEvent
    {
        public UnitActor From { get; set; }
        public UnitActor To { get; set; }
        public float Amount { get; set; }
        public string Effect { get; set; }
    }

    public struct ShieldEvent : IUnitEvent
    {
        public UnitActor From { get; set; }
        public UnitActor To { get; set; }
        public float Amount { get; set; }
        public string Effect { get; set; }
        public float Duration { get; set; }
    }

    public struct RestrictionEvent : IUnitEvent
    {
        public UnitActor From { get; set; }
        public UnitActor To { get; set; }
        public CombatController.Restrictions Type { get; set; }
        public float Amount { get; set; }
        public string Effect { get; set; }
        public float Duration { get; set; }
    }

    public struct StatusEffectEvent : IUnitEvent
    {
        public UnitActor From { get; set; }
        public UnitActor To { get; set; }
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

    public struct ProjectileEvent
    {
        public string Prefab;
        public UnitActor From;
        public UnitActor To;
    }
}
