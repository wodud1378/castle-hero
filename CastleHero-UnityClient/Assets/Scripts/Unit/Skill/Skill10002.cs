using System;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;

namespace RGLabs.Unit.Skill
{
    public class Skill10002 : ActiveSkill
    {
        private enum Stat
        {
            CenterAtk = 0,
            AroundAtk,
            Debuff,
        }

        protected override void OnExecute()
        {
            var onEnemy = BuildExecutionOnEnemy();
            if (onEnemy == null)
                return;

            if (!TryGetQuantityParameter(0, out int quantity))
                return;

            if (!TryUpdateAroundCenter(quantity))
                return;
            
            if (TryGetStatusParameter(Stat.CenterAtk, out var type, out var value))
            {
                var center = aroundCenter.center;
                PublishAtk(aroundCenter.center, DamageType.Normal, GetAmount(type, value));
                PlayEffect(0, center.position);
            }
            
            foreach (var unit in aroundCenter.units)
            {
                onEnemy.Invoke(unit);
            }
        }

        private Action<UnitBehaviour> BuildExecutionOnEnemy()
        {
            Action<UnitBehaviour> action = null;
            if (TryGetStatusParameter(Stat.AroundAtk, out var type, out var value))
            {
                float atkAmount = GetAmount(type, value);
                action += (x) => PublishAtk(x, DamageType.Normal, atkAmount);
            }

            if (TryGetStatusParameter(Stat.Debuff, out type, out value))
            {
                float debuffAmount = GetAmount(type, value);
                float duration = Data.duration;
                action += x => { PublishDebuff(x, Status.Type.AtkSpeed, debuffAmount, duration, true); };
            }

            return action;
        }
    }
}