using System;
using RGLabs.Unit.Behaviours;
using UniRx;
using UnityEngine;

namespace RGLabs.InGame.System
{
    public class WaitRecover : IDisposable
    {
        public UnitBehaviour behaviour;
        public Vector2 position;
        public float leftTime;

        private IDisposable _subscription;
        private ReactiveCollection<WaitRecover> _root;

        public void Bind(ReactiveCollection<WaitRecover> root, IDisposable subscription)
        {
            _root = root;
            _subscription = subscription;
        }

        public void Dispose()
        {
            _root?.Remove(this);
            _subscription?.Dispose();
        }
    }

    public enum DamageType
    {
        Normal,
        Debuff
    }
    

    public interface IModifier
    {
        public UnitBehaviour From { get; }
        public UnitBehaviour To { get; }
        public float Amount { get; }
    }

    public interface IModifyResult
    {
        public IModifier Event { get; }
    }

    public struct AtkEvent : IModifier
    {
        public DamageType Type { get; set; }
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public float Amount { get; set; }
    }

    public struct HealEvent : IModifier
    {
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public float Amount { get; set; }
    }

    public struct ShieldEvent : IModifier
    {
        public UnitBehaviour From { get; set; }
        public UnitBehaviour To { get; set; }
        public float Amount { get; set; }
        public float Duration { get; set; }
    }

    public struct AtkResult : IModifyResult
    {
        public IModifier Event { get; set; }

        public bool IsCritical { get; set; }
        
        public float Protected { get; set; }
    }

    public struct HealResult  : IModifyResult
    {
        public IModifier Event { get; set; }
    }

    public struct ShieldResult : IModifyResult
    {
        public IModifier Event { get; set; }
    }
}