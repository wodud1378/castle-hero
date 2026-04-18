using System;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Components;
using CastleHero.GamePlay.Unit.Finding;
using CastleHero.GamePlay.Unit.Skill.Components.Factory;

namespace CastleHero.GamePlay.Unit.Skill
{
    public static class SkillHelper
    {
        private static readonly SkillBuilder Builder = new();

        public static ISkill Attach(this CombatController combat, int id, int lv)
        {
            Func<SkillBuilder, ISkill> buildMethod;
            var owner = combat.GetComponent<UnitActor>();
            if (!Builder.StartBuild(owner, id, lv))
                return null;
            
            switch (id)
            {
                case 10001: buildMethod = Build10001; break;
                case 10002: buildMethod = Build10002; break;
                case 10003: buildMethod = Build10003; break;
                case 10004: buildMethod = Build10004; break;
                case 10006: buildMethod = Build10006; break;
                case 10007: buildMethod = Build10007; break;
                case 10010: buildMethod = Build10010; break;
                case 10011: buildMethod = Build10011; break;
                case 10013: buildMethod = Build10013; break;
                case 10016: buildMethod = Build10016; break;
                case 10019: buildMethod = Build10019; break;
                case 10020: buildMethod = Build10020; break;
                case 10021: buildMethod = Build10021; break;
                case 10023: buildMethod = Build10023; break;
                case 10025: buildMethod = Build10025; break;
                case 10031: buildMethod = Build10031; break;
                case 10032: buildMethod = Build10032; break;
                case 10033: buildMethod = Build10033; break;
                case 10034: buildMethod = Build10034; break;
                case 20009: buildMethod = Build20009; break;
                case 20012: buildMethod = Build20012; break;
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
        
        private static ISkill Build10010(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Enemy, 0)
                .BuildActiveSkill();
        }
        
        private static ISkill Build10011(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Arc, Targeting.Enemy, 0)
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
        
        private static ISkill Build10019(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Alley, 0)
                .BuildActiveSkill();
        }
        
        private static ISkill Build10020(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
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
        
        private static ISkill Build10031(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Enemy, 1)
                .SetBound(IDetection.Option.Circle, default, 0)
                .BuildActiveSkill();
        }
        
        private static ISkill Build10032(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Enemy, 0)
                .BuildActiveSkill();
        }
        
        private static ISkill Build10033(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Enemy, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build10034(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Alley, 0)
                .BuildActiveSkill();
        }

        private static ISkill Build20009(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Alley, 0)
                .BuildActiveSkill();
        }
        
        private static ISkill Build20012(SkillBuilder builder)
        {
            return builder
                .SetTargeting(Targeting.Self, 1)
                .SetBound(IDetection.Option.Circle, Targeting.Alley, 0)
                .BuildActiveSkill();
        }
    }
}