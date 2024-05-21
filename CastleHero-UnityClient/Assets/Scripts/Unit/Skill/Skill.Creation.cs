using System;
using RGLabs.Common.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Skill.Components;

namespace RGLabs.Unit.Skill
{
    public static class SkillHelper
    {
        private static readonly TargetFinderFactory FinderFactory = new();
        private static readonly BoundFactory BoundFactory = new();

        public static ISkill Attach(this UnitBehaviour unit, int id, int lv)
        {
            var skill = Create(unit, id, lv);
            switch (id)
            {
                case 10021:
                    Build10021(skill);
                    break;
            }

            return skill;
        }

        private static ISkill Create(UnitBehaviour owner, int id, int lv)
        {
            var db = Context.currentBehaviour.db.skills;
            int skillId = (id * 10) + lv;
            if (!db.TryFind(skillId, out var entity))
                return null;

            var type = Type.GetType($"RGLabs.Unit.Skill.Skill{id}");
            if (type == null)
                return null;

            var skill = (ISkill)Activator.CreateInstance(type);
            skill.Owner = owner;
            skill.Data = entity;
            return skill;
        }

        private static void Build10021(ISkill skill)
        {
            var owner = skill.Owner;
            var targeting = FinderFactory.GetTargeting(owner, TargetFinderFactory.Option.Enemy,
                skill.Data.targetQty[1]);

            var bound = BoundFactory.GetBound(owner, BoundFactory.Option.Circle);
            
            skill.Targeting = targeting;
            skill.Bound = bound;
        }
    }
}