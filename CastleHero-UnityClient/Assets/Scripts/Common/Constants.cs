using System.Collections.Generic;
using RGLabs.Unit.Skill.Global;

namespace RGLabs.Common
{
    public static class Constants
    {
        public static readonly int BufferSize = 50;
        public static readonly float HitEffectDuration = 0.15f;

        public static readonly float DragDistanceThreshold = 0.3f;
        
        public static string ColoredStringTag = "<color={0}>{1}</color>";

        public static readonly int BarricadeId = 10000;
        public static readonly string BarricadeIcon = "Common/Portrait/Character_10000.png";
        public static readonly string BarricadePrefab = "Character_10000/Character_10000.prefab";

        public static string GoldIcon = "Common/Icon/Icon_Gold.png";
        public static string DiaIcon = "Common/Icon/Icon_Diamond.png";
        public static string ExpIcon = "Common/Icon/Icon_Exp.png";
        public static float DefaultObjectAngle = 0;

        public static readonly Dictionary<GlobalSkill.Type, string> GlobalSkillIcon = new()
        {
            { GlobalSkill.Type.Shield, "Common/Icon/Icon_Skill_Global_03.png" },
            { GlobalSkill.Type.Damage, "Common/Icon/Icon_Skill_Global_03.png" },
            { GlobalSkill.Type.Sturn, "Common/Icon/Icon_Skill_Global_03.png" },
            { GlobalSkill.Type.Heal, "Common/Icon/Icon_Skill_Global_03.png" },
        };

        public static readonly Dictionary<GlobalSkill.Type, string> GlobalSkillEffect = new()
        {
            { GlobalSkill.Type.Shield, "Effect_Global_03" },
            { GlobalSkill.Type.Damage, "Effect_Global_03" },
            { GlobalSkill.Type.Sturn, "Effect_Global_03" },
            { GlobalSkill.Type.Heal, "Effect_Global_03" },
        };

        public static string ManaDropEffect = "Effect_ManaStone";
        public static string DeadEffect = "UnitEffect/Dead/Dead.prefab";
        public static string RecoverEffect = "UnitEffect/Recover/Recover.prefab";
        public static string SpawnEffect = "UnitEffect/Spawn/Spawn.prefab";

        public static readonly int PaidDiaId = 1;
        public static readonly int FreeDiaId = 2;        
        public static readonly int GoldId = 3;
    }
}