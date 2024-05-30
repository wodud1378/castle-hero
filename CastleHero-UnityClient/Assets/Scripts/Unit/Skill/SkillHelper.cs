using System;
using RGLabs.Unit.Components;
using RGLabs.Unit.Finding;
using RGLabs.Unit.Skill.Components.Factory;

namespace RGLabs.Unit.Skill
{
    public static class SkillHelper
    {
        private static readonly SkillBuilder Builder = new();

        public static ISkill Attach(this UnitCore unit, int id, int lv)
        {
            Func<SkillBuilder, ISkill> buildMethod;
            if (!Builder.StartBuild(unit.owner, id, lv))
                return null;
            
            switch (id)
            {
                case 10001: buildMethod = Build10001; break;
                case 10002: buildMethod = Build10002; break;
                case 10003: buildMethod = Build10003; break;
                case 10004: buildMethod = Build10004; break;
                case 10006: buildMethod = Build10006; break;
                case 10007: buildMethod = Build10007; break;
                case 10013: buildMethod = Build10013; break;
                case 10016: buildMethod = Build10016; break;
                case 10021: buildMethod = Build10021; break;
                case 10023: buildMethod = Build10023; break;
                case 10025: buildMethod = Build10025; break;
                case 10034: buildMethod = Build10034; break;
                default:
                    return null;
            }

            return buildMethod.Invoke(Builder);
        }
        
        private static ISkill Build10001(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Both, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10002(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Enemy, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Enemy, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10003(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Enemy, 1)
                .BuildActiveSkill();
        }

        private static ISkill Build10004(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Enemy, 1)
                .SetBound(IDetection.Option.Box, Targeting.Enemy, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10006(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Alley, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10007(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Enemy, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10013(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Enemy, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Enemy, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10016(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Enemy, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10021(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Enemy, 1)
                .BuildActiveSkill();
        }

        private static ISkill Build10023(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Alley, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10025(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Alley, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10034(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Alley, 0)
                .BuildActiveSkill();
        }
    }
}