using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Unit
{
    public abstract class CachedValue
    {
        protected abstract float Value { get; }

        protected float Cached { get; private set; }

        public virtual void Update() => Cached = Value;

        public static implicit operator float(CachedValue it) => it.Cached;

        public static float operator +(CachedValue a, CachedValue b) => (float)a + (float)b;
        public static float operator -(CachedValue a, CachedValue b) => (float)a - (float)b;
        public static float operator *(CachedValue a, CachedValue b) => (float)a * (float)b;
        public static float operator /(CachedValue a, CachedValue b) => (float)a / (float)b;
    }

    public struct TimedValue
    {
        public float leftTime;
        public float value;
    }

    public class Multiplier : CachedValue
    {
        public event Action<float, float> OnChanged;

        private readonly List<TimedValue> _increase = new();
        private readonly List<TimedValue> _decrease = new();

        private float _default;

        protected override float Value
        {
            get
            {
                float increase = _increase.Sum(t => t.value);
                float decrease = _decrease.Sum(t => t.value);

                return _default + increase + decrease;
            }
        }

        public void Init(float val)
        {
            _default = val;

            _increase.Clear();
            _decrease.Clear();

            Update();
        }

        public void Increase(float val, float time) => Add(_increase, val, time);

        public void Decrease(float val, float time) => Add(_decrease, val, time);

        public override void Update()
        {
            float legacy = Cached;
            var deltaTime = Time.deltaTime;
            _increase.RemoveAll((item) => item.leftTime - deltaTime <= 0);
            _decrease.RemoveAll((item) => item.leftTime - deltaTime <= 0);

            base.Update();
            OnChanged?.Invoke(legacy, Cached);
        }

        private void Add(List<TimedValue> target, float val, float time = float.MaxValue)
        {
            var timedVal = new TimedValue
            {
                leftTime = time,
                value = val
            };

            target.Add(timedVal);
        }
    }

    public class Ability : CachedValue
    {
        public readonly Multiplier multiplier = new();
        public float origin;

        public virtual void Init(IList<Ability> root = null, float origin = 0f, float initialMul = 1f)
        {
            this.origin = origin;

            multiplier.Init(initialMul);

            root?.Add(this);
        }

        protected override float Value => multiplier * origin;

        public static implicit operator float(Ability it) => it.Value;
    }

    public class Hp : Ability
    {
        public float Max => Value;

        public float Left { get; private set; }

        public void Increase(float value) => Left = Mathf.Min(Max, Left + value);

        public void Decrease(float value) => Left = Mathf.Max(0, Left - value);

        public Hp() => multiplier.OnChanged += OnMultiplierChanged;

        public override void Init(IList<Ability> root = null, float origin = 0, float initialMul = 1)
        {
            base.Init(root, origin, initialMul);

            Left = Max;
        }

        private void OnMultiplierChanged(float legacy, float current)
        {
            float diff = current - legacy;
            if (diff > 0f)
            {
                float heal = Max * diff;
                Left = Mathf.Min(Max, Left + heal);
            }
        }

        public static implicit operator float(Hp it) => it.Left;
    }

    public class Status
    {
        public readonly Hp hp = new();
        public readonly Ability speed = new();
        public readonly Ability atk = new();
        public readonly Ability moveRange = new();
        public readonly Ability atkRange = new();
        public readonly Ability atkSpeed = new();
        public readonly Ability critical = new();
        public readonly Ability criticalAtk = new();
        public readonly Ability recovery = new();

        private List<Ability> _abilities;
        

        public void Init(UnitEntity data)
        {
            _abilities = new List<Ability>();

            hp.Init(_abilities, data.hp);
            speed.Init(_abilities, data.speed);
            atk.Init(_abilities, data.atk);
            moveRange.Init(_abilities, data.moveRange);
            atkRange.Init(_abilities, data.atkRange);
            atkSpeed.Init(_abilities, data.atkSpeed);
            critical.Init(_abilities, data.critical);
            criticalAtk.Init(_abilities, data.criticalAtk);
            recovery.Init(_abilities, data.recovery);
        }

        public void Update()
        {
            foreach (var ability in _abilities)
            {
                ability.Update();
            }
        }
    }
}