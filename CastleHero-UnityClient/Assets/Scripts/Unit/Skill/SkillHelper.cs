using System;
using RGLabs.Common.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Components;
using RGLabs.Unit.Skill.Components;

namespace RGLabs.Unit.Skill
{
    public static class SkillHelper
    {
        private static readonly TargetFinderFactory FinderFactory = new();
        private static readonly BoundFactory BoundFactory = new();

        public static ISkill Attach(this UnitCore unit, int id, int lv)
        {
            var skill = Create(unit.owner, id, lv);
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

        private static void Build10001(ISkill skill)
        {
            var data = skill.Data;
            var owner = skill.Owner;
            skill.Targeting = FinderFactory.GetTargeting(owner, TargetFinderFactory.Option.Self, 1);
            skill.Bound = BoundFactory.GetBound(BoundFactory.Option.Circle);
            skill.Cycle = new CoolTime(owner, data.coolTime);
            skill.Runner = new AnimationRunner(owner.Core.animationEvent);
            skill.Init();
        }

        private static void Build10021(ISkill skill)
        {
            var data = skill.Data;
            var owner = skill.Owner;
            skill.Targeting = FinderFactory.GetTargeting(owner, TargetFinderFactory.Option.Enemy, 1);
            skill.Bound = BoundFactory.GetBound(BoundFactory.Option.Circle);
            skill.Cycle = new CoolTime(owner, data.coolTime);
            skill.Runner = new AnimationRunner(owner.Core.animationEvent);
            skill.Init();
        }

        private static void Build(ISkill skill, TargetFinderFactory.Option targetingOption, BoundFactory.Option boundOption)
        {
            var data = skill.Data;
            var owner = skill.Owner;
            skill.Targeting = FinderFactory.GetTargeting(owner, targetingOption, 1);
            skill.Bound = BoundFactory.GetBound(BoundFactory.Option.Circle);
            skill.Cycle = new CoolTime(owner, data.coolTime);
            skill.Runner = new AnimationRunner(owner.Core.animationEvent);
            skill.Init();
        }
    }
}