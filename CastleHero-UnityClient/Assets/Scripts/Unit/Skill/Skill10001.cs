using System;
using System.Collections.Generic;
using RGLabs.InGame.System;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;

namespace RGLabs.Unit.Skill
{
    public class Skill10001 : ActiveSkill
    {
        public enum Parameter
        {
            Heal = 0,
            Atk,
        }
        
        protected override void OnExecute()
        {
            if (TryBuildExecution(out var onAlley, out var onEnemy))
                return;
            
            var center = Targeting.Targets[0];
            Bound.FindTargets(center.position, Data.range, default)
                .ForEach(x =>
                {
                    if (x.IsAlley(Owner))
                        onAlley.Invoke(x);
                    else
                        onEnemy.Invoke(x);
                });
        }

        private bool TryBuildExecution(out Action<UnitBehaviour> onAlley, out Action<UnitBehaviour> onEnemy)
        {
            onAlley = null;
            onEnemy = null;

            if (TryGetStatusParameter(Parameter.Heal, out var type1, out var value1))
            {
                float healAmount = WithOwner(type1, value1);
                onAlley += (x) =>
                {
                    x.Core.SetInvincible(Data.duration);

                    PublishHeal(x, healAmount);
                };
            }

            if (TryGetStatusParameter(Parameter.Heal, out var type2, out var value2))
            {
                float atkAmount = WithOwner(type2, value2);
                onEnemy += x=> PublishAtk(x, DamageType.Normal, atkAmount);
            }

            return onAlley != null || onEnemy != null;
        }
    }
}