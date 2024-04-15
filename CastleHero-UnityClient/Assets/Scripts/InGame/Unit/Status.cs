using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.Unit
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

        private readonly List<TimedValue> _increase;
        private readonly List<TimedValue> _decrease;

        private readonly float _default;

        protected override float Value
        {
            get
            {
                float increase = _increase.Sum(t => t.value);
                float decrease = _decrease.Sum(t => t.value);

                return _default + increase + decrease;
            }
        }

        public Multiplier(float initialVal = 0f)
        {
            _default = initialVal;

            _increase = new();
            _decrease = new();

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

    [Serializable]
    public class Ability : CachedValue
    {
        public readonly Multiplier multiplier;
        public readonly float origin;

        public Ability(IList<Ability> root = null, float origin = 0f, float initialMul = 1f)
        {
            this.origin = origin;

            multiplier = new(initialMul);

            root?.Add(this);
        }

        protected override float Value => multiplier * origin;

        public static implicit operator float(Ability it) => it.Value;
    }

    [Serializable]
    public class Hp : Ability
    {
        public float Max => Value;

        [field: SerializeField] public float Left { get; private set; }

        public void Increase(float value) => Left = Mathf.Min(Max, Left + value);

        public void Decrease(float value) => Left = Mathf.Max(0, Left - value);

        public Hp(IList<Ability> root, float origin = 0f, float initialMul = 1f) : base(root, origin, initialMul)
        {
            multiplier.OnChanged += OnMultiplierChanged;
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

    [Serializable]
    public class Status
    {
        public Hp hp;
        public Ability speed;
        public Ability atk;
        public Ability moveRange;
        public Ability atkRange;
        public Ability atkSpeed;
        public Ability critical;
        public Ability criticalAtk;

        private List<Ability> _abilities;

        public void Init(UnitEntity data)
        {
            _abilities = new List<Ability>();
            
            hp = new Hp(_abilities, data.hp);
            speed = new Ability(_abilities, data.speed);
            atk = new Ability(_abilities, data.atk);
            moveRange = new Ability(_abilities, data.moveRange);
            atkRange = new Ability(_abilities, data.atkRange);
            atkSpeed = new Ability(_abilities, data.atkSpeed);
            critical = new Ability(_abilities, data.critical);
            criticalAtk = new Ability(_abilities, data.criticalAtk);
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