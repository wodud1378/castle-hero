using RGLabs.InGame.System;
using RGLabs.Utility;

namespace RGLabs.Unit.Skill
{
    public class Skill10021 : ActiveSkill
    {
        private enum Step
        {
            Buff,
            Atk
        }

        protected override void OnExecute()
        {
            BuffsOnGroup();
            AttackEnemies();
        }

        private void BuffsOnGroup()
        {
            if (!TryGetGroupParameter(0, out int group))
                return;

            if (!TryGetStatusParameter(Step.Buff, out var type, out var value))
                return;

            if (!TryGetQuantityParameter(0, out int quantity))
                return;
            
            TryGetEffectPrefab(0, out string buffEff);
            
            var characters = Characters(x => x.Data.group == group, quantity);
            foreach (var character in characters)
            {
                PublishBuff(character, type, value, Data.duration, true, buffEff);
            }
        }

        private void AttackEnemies()
        {
            if (!Targeting.HasTargets())
                return;

            if (!TryGetStatusParameter(Step.Atk, out var type, out var value))
                return;

            TryGetEffectPrefab(1, out string atkEff);
            
            float atkAmount = GetAmount(type, value);
            Targeting.Targets.ForEach(x => PublishAtk(x, DamageType.Normal, atkAmount, atkEff));
        }
    }
}