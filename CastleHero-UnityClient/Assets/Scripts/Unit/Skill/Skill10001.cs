using System;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;

namespace RGLabs.Unit.Skill
{
    public class Skill10001 : ActiveSkill
    {
        private enum Step
        {
            Heal = 0,
            Atk,
        }

        private enum Qty
        {
            Heal = 0,
            Invincible = 1,
            Atk = 2,
        }

        protected override void OnExecute()
        {
            if (!TryBuildExecution(out var onAlley, out var onEnemy))
                return;

            if (!TryUpdateAroundCenter())
                return;

            PlayEffect(0, aroundCenter.center.position);
            foreach (var unit in aroundCenter.around)
            {
                if (unit.IsAlley(Owner))
                    onAlley.Invoke(unit);
                else
                    onEnemy.Invoke(unit);
            }
        }

        private bool TryBuildExecution(out Action<UnitBehaviour> onAlley, out Action<UnitBehaviour> onEnemy)
        {
            onAlley = null;
            onEnemy = null;

            if (TryGetStatusParameter(Step.Heal, out var type1, out var value1) &&
                TryGetQuantityParameter(Qty.Heal, out int healQty) &&
                TryGetQuantityParameter(Qty.Invincible, out int invincibleQty))
            {
                int healPublished = 0;
                int invinciblePublished = 0;
                float healAmount = GetAmount(type1, value1);

                TryGetEffectPrefab(1, out var effect);
                
                onAlley += x =>
                {
                    if (healPublished++ < healQty)
                        x.Core.SetInvincible(Data.duration);

                    if (invinciblePublished++ < invincibleQty)
                        PublishHeal(x, healAmount, effect);
                };
            }

            if (TryGetStatusParameter(Step.Atk, out var type2, out var value2) &&
                TryGetQuantityParameter(Qty.Atk, out int atkQty))
            {
                int atk = 0;
                float atkAmount = GetAmount(type2, value2);
                onEnemy += x =>
                {
                    if (atk++ < atkQty)
                        PublishAtk(x, DamageType.Normal, atkAmount);
                };
            }

            return onAlley != null || onEnemy != null;
        }
    }
}