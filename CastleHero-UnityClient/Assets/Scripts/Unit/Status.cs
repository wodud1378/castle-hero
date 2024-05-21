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

    public class TimedValue
    {
        public float leftTime;
        public float value;
    }

    public class AdjustValue : CachedValue
    {
        public event Action<float, float> OnChanged;

        private readonly List<TimedValue> _increaseMul = new();
        private readonly List<TimedValue> _decreaseMul = new();
        
        private float _default;

        protected override float Value
        {
            get
            {
                float increase = _increaseMul.Sum(t => t.value);
                float decrease = _decreaseMul.Sum(t => t.value);

                return _default + increase - decrease;
            }
        }

        public void Init(float val)
        {
            _default = val;

            _increaseMul.Clear();
            _decreaseMul.Clear();

            Update();
        }

        public void Increase(float val, float time) => Add(_increaseMul, val, time);

        public void Decrease(float val, float time) => Add(_decreaseMul, val, time);

        public override void Update()
        {
            float legacy = Cached;
            var deltaTime = Time.deltaTime;

            _increaseMul.ForEach(x=> x.leftTime -= deltaTime);
            _decreaseMul.ForEach(x => x.leftTime -= deltaTime);
            
            _increaseMul.RemoveAll((item) => item.leftTime - deltaTime <= 0);
            _decreaseMul.RemoveAll((item) => item.leftTime - deltaTime <= 0);

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
        public readonly AdjustValue multiplyAdjust = new();
        public readonly AdjustValue fixedAdjust = new();
        public float origin;

        public virtual void Init(IList<Ability> root = null, float origin = 0f, float initialMul = 1f)
        {
            this.origin = origin;

            multiplyAdjust.Init(initialMul);
            fixedAdjust.Init(0);

            root?.Add(this);
        }

        protected override float Value => (multiplyAdjust * origin) + fixedAdjust;

        public static implicit operator float(Ability it) => it.Value;
    }

    public class Hp : Ability
    {
        public float Max => Value;

        public float Left { get; private set; }

        public void Increase(float value) => Left = Mathf.Min(Max, Left + value);

        public void Decrease(float value) => Left = Mathf.Max(0, Left - value);

        public Hp()
        {
            multiplyAdjust.OnChanged += OnMultiplyAdjustChanged;
            fixedAdjust.OnChanged += OnFixedAdjustChanged;
        }

        public override void Init(IList<Ability> root = null, float origin = 0, float initialMul = 1)
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

        public static implicit operator float(Hp it) => it.Left;
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
        public readonly Ability atk = new();
        public readonly Ability critical = new();
        public readonly Ability criticalAtk = new();
        public readonly Ability atkSpeed = new();
        public readonly Ability speed = new();
        public readonly Ability atkRange = new();
        public readonly Ability moveRange = new();
        public readonly Ability recovery = new();

        private List<Ability> _abilities;

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

        public void Init(UnitEntity data, int lv, UnitLevelEntity levelData)
        {
            _abilities = new List<Ability>();

            hp.Init(_abilities, data.hp + (lv * levelData.hp));
            atk.Init(_abilities, data.atk + (lv * levelData.atk));
            critical.Init(_abilities, data.critical + (lv * levelData.critical));
            criticalAtk.Init(_abilities, data.criticalAtk + (lv * levelData.criticalAtk));
            atkSpeed.Init(_abilities, data.atkSpeed + (lv * levelData.atkSpeed));
            speed.Init(_abilities, data.speed + (lv * levelData.speed));
            atkRange.Init(_abilities, data.atkRange + (lv * levelData.atkRange));
            moveRange.Init(_abilities, data.moveRange + (lv * levelData.moveRange));
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