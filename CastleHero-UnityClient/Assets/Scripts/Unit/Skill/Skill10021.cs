using RGLabs.InGame.System;
using RGLabs.Utility;

namespace RGLabs.Unit.Skill
{
    public class Skill10021 : ActiveSkill
    {
        public enum Parameter
        {
            SpeedBuff,
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

            if (!TryGetStatusParameter(Parameter.SpeedBuff, out var type, out var value))
                return;

            var characters = Characters(x => x.Data.team == group);
            float increaseTime = Data.duration;
            float increaseValue = WithOwner(type, value);
            foreach (var character in characters)
            {
                character.status.speed.fixedAdjust.Increase(increaseValue, increaseTime);
            }
        }

        private void AttackEnemies()
        {
            if (!Targeting.HasTargets())
                return;

            if (!TryGetStatusParameter(Parameter.Atk, out var type, out var value))
                return;

            float atkAmount = WithOwner(type, value);
            Targeting.Targets.ForEach(x => PublishAtk(x, DamageType.Normal, atkAmount));
        }
    }
}