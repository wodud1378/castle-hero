using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data.Model;
using RGLabs.InGame.Effects;
using RGLabs.InGame.Effects.Behaviours;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit
{
    public interface IUpdate
    {
        public void Update();
    }

    public abstract class CachedValue : IUpdate
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

    public class TimedValue
    {
        public float leftTime;
        public float value;
    }

    public class AdjustValue : CachedValue
    {
        public event Action<float, float> OnChanged;

        private readonly List<TimedValue> _timedIncrease = new();
        private readonly List<TimedValue> _timedDecrease = new();

        private float _increase;
        private float _decrease;

        private float _default;

        protected override float Value
        {
            get
            {
                float increase = _timedIncrease.Sum(t => t.value) + _increase;
                float decrease = _timedDecrease.Sum(t => t.value) + _decrease;

                return _default + increase - decrease;
            }
        }

        public void Init(float val)
        {
            _default = val;

            _timedIncrease.Clear();
            _timedDecrease.Clear();

            Update();
        }

        public void Increase(float val) => _increase += val;

        public void Decrease(float val) => _decrease += val;

        public void Increase(float val, float time) => Add(_timedIncrease, val, time);

        public void Decrease(float val, float time) => Add(_timedDecrease, val, time);

        public override void Update()
        {
            float legacy = Cached;
            var deltaTime = Time.deltaTime;

            _timedIncrease.ForEach(x => x.leftTime -= deltaTime);
            _timedDecrease.ForEach(x => x.leftTime -= deltaTime);

            _timedIncrease.RemoveAll((item) => item.leftTime <= 0);
            _timedDecrease.RemoveAll((item) => item.leftTime <= 0);

            base.Update();
            OnChanged?.Invoke(legacy, Cached);
        }

        private void Add(List<TimedValue> target, float val, float time)
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
        public readonly AdjustValue multiplyAdjust = new();
        public readonly AdjustValue fixedAdjust = new();
        public float origin;

        public virtual void Init(IList<IUpdate> root = null, float origin = 0f, float initialMul = 1f)
        {
            this.origin = origin;

            multiplyAdjust.Init(initialMul);
            fixedAdjust.Init(0);

            root?.Add(this);
        }

        public override void Update()
        {
            multiplyAdjust.Update();
            fixedAdjust.Update();

            base.Update();
        }

        protected override float Value => (multiplyAdjust * origin) + fixedAdjust;

        public static implicit operator float(Ability it) => it.Value;
    }

    public class Shield : CachedValue
    {
        private readonly List<KeyValuePair<TimedValue, IEffect>> _timedValues = new();
        private const float NotTimedValue = float.MinValue;

        private float _value;

        public void Init(IList<IUpdate> root) => root.Add(this);

        protected override float Value => _timedValues.Sum(x => x.Key.value);
        
        public void Increase(float val, IEffect effect)
        {
            _timedValues.Add(new KeyValuePair<TimedValue, IEffect>(
                new TimedValue
                {
                    leftTime = NotTimedValue,
                    value = val
                },
                effect)
            );
        }

        public void Increase(float val, float time, IEffect effect)
        {
            _timedValues.Add(new KeyValuePair<TimedValue, IEffect>(
                new TimedValue
                {
                    leftTime = time,
                    value = val
                },
                effect)
            );
        }

        public void Decrease(float val, out float @protected, out float left)
        {
            left = val;
            @protected = 0f;
            foreach (var timedValue in _timedValues)
            {
                float diff = timedValue.Key.value - left;
                float damage;
                if (diff < 0)
                {
                    damage = timedValue.Key.value;
                    left = Mathf.Abs(diff);
                }
                else
                {
                    damage = diff;
                }

                timedValue.Key.value -= damage;
                @protected += damage;
                left = Mathf.Abs(diff);
            }
        }

        public override void Update()
        {
            float deltaTime = Time.deltaTime;
            _timedValues.ForEach(x =>
            {
                if (x.Key.leftTime <= NotTimedValue)
                    return;

                x.Key.leftTime -= deltaTime;
            });
            
            _timedValues.RemoveAll((item) =>
            {
                bool isRemove = item.Key.value <= 0 || item.Key.leftTime is > NotTimedValue and <= 0f;
                if (isRemove && item.Value != null)
                {
                    item.Value.Stop();
                }

                return isRemove;
            });

            base.Update();
        }
    }

    public class Hp : Ability
    {
        public float Max => Value;

        public float Left { get; private set; }

        public void Increase(float value) => Left = Mathf.Min(Mathf.Max(Max, Left), Left + value);

        public void Decrease(float value) => Left = Mathf.Max(0, Left - value);

        public Hp()
        {
            multiplyAdjust.OnChanged += OnMultiplyAdjustChanged;
            fixedAdjust.OnChanged += OnFixedAdjustChanged;
        }

        public override void Init(IList<IUpdate> root = null, float origin = 0, float initialMul = 1)
        {
            base.Init(root, origin, initialMul);

            Left = Max;
        }

        private void OnMultiplyAdjustChanged(float legacy, float current)
        {
            float diff = current - legacy;
            if (diff > 0f)
            {
                float heal = Max * diff;
                Left = Mathf.Min(Max, Left + heal);
            }
        }

        private void OnFixedAdjustChanged(float legacy, float current)
        {
            float diff = current - legacy;
            if (diff > 0f)
            {
                Left = Mathf.Min(Max, Left + diff);
            }
        }
    }

    public class Status
    {
        public enum Type
        {
            Hp = 0,
            Atk,
            Critical,
            CriticalAtk,
            AtkSpeed,
            MoveSpeed,
            AtkRange,
            MoveRange,
            Recovery,
        }

        public readonly Dictionary<Type, Ability> abilities;

        public Ability this[Type type] => abilities.GetValueOrDefault(type);

        public readonly Hp hp = new();
        public readonly Shield shield = new();
        public readonly Ability atk = new();
        public readonly Ability critical = new();
        public readonly Ability criticalAtk = new();
        public readonly Ability atkSpeed = new();
        public readonly Ability speed = new();
        public readonly Ability atkRange = new();
        public readonly Ability moveRange = new();
        public readonly Ability recovery = new();

        private List<IUpdate> _updates;

        public Status()
        {
            abilities = new Dictionary<Type, Ability>
            {
                { Type.Hp, hp },
                { Type.Atk, atk },
                { Type.Critical, critical },
                { Type.CriticalAtk, criticalAtk },
                { Type.AtkSpeed, atkSpeed },
                { Type.MoveSpeed, speed },
                { Type.AtkRange, atkRange },
                { Type.MoveRange, moveRange },
                { Type.Recovery, recovery },
            };
        }

        public void Init(UnitEntity data)
        {
            _updates = new List<IUpdate>();

            hp.Init(_updates, data.hp);
            atk.Init(_updates, data.atk);
            critical.Init(_updates, data.critical);
            criticalAtk.Init(_updates, data.criticalAtk);
            atkSpeed.Init(_updates, data.atkSpeed);
            speed.Init(_updates, data.speed);
            atkRange.Init(_updates, data.atkRange);
            moveRange.Init(_updates, data.moveRange);
            recovery.Init(_updates, data.recovery);
            shield.Init(_updates);
        }

        public void Update()
        {
            foreach (var ability in _updates)
            {
                ability.Update();
            }
        }
    }
}