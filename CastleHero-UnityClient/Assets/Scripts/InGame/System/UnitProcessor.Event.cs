using System;
using RGLabs.Unit;
using RGLabs.Unit.Behaviours;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.System
{
    public class WaitRecover : IDisposable
    {
        public UnitBehaviour behaviour;
        public Vector2 position;
        public float time;
        
        public readonly ReactiveProperty<float> leftTime = new();
        public readonly ReactiveProperty<(float left, float total)> summary = new();

        private IDisposable _subscription;
        private ReactiveCollection<WaitRecover> _root;
        
        public void Bind(ReactiveCollection<WaitRecover> root, IDisposable subscription)
        {
            leftTime
                .Select(x => (x, time))
                .DistinctUntilChanged()
                .Subscribe(x => summary.Value = x);

            _root = root;
            _subscription = subscription;
        }

        public void Dispose()
        {
            _root?.Remove(this);
            _subscription?.Dispose();

            summary.Dispose();
            leftTime.Dispose();
        }
    }

    public enum DamageType
    {
        Normal,
        Debuff
    }

    public interface IUnitEvent
    {
        public UnitBehaviour From { get; }
        public UnitBehaviour To { get; }
        public float Amount { get; }
        public string Effect { get; }
    }

    public interface IUnitEventResult
    {
        public IUnitEvent Event { get; }
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