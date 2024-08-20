using System;
using RGLabs.Common;
using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public enum CastleSkillType
    {
        Shield,
        Damage,
        Sturn,
        Heal
    }
    
    public struct CastleSkillParameter
    {
        public CastleSkillType type;
        public string icon;
        public float value;
        public float radius;
        public float coolTime;
        public string centerEffect;
        public string unitEffect;
        public string sound;
    }
    
    public struct CastleEntity : IEntity
    {
        [DataField("Castle_Level")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }

        [DataField("Castle_Gold")]
        public int lvUpPrice;

        [DataField("Castle_Slot")] 
        public int maxCharacter;

        [DataField("Castle_HP")] 
        public float hp;

        [DataField("Castle_Object_Value")]
        public int barricadeCount;

        [DataField("Castle_Object_HP")]
        public int barricadeHp;

        [DataField("Castle_Skill")] 
        public string[] skills;

        [DataField("Castle_Skill_Icon")] 
        public string[] skillIcons;

        [DataField("Castle_Skill_Value")] 
        public float[] skillValues;

        [DataField("Castle_Skill_Range")] 
        public float[] skillRanges;

        [DataField("Castle_Skill_Time")] 
        public float[] skillCoolTimes;

        [DataField("Castle_Skill_Sound")] 
        public string[] skillSounds;
        
        [DataField("Castle_Effect")] 
        public string[] skillEffects;
        
        [DataField("Castle_Effect_Unit")] 
        public string[] unitEffects;

        public CastleSkillParameter[] SkillParameters()
        {
            var array = new CastleSkillParameter[4];
            for (int i = 0; i < 4; ++i)
            {
                var type = Enum.Parse<CastleSkillType>(skills[i]);
                array[i] = new CastleSkillParameter
                {
                    type = type,
                    icon = skillIcons[i],
                    value = skillValues[i],
                    radius = skillRanges[i],
                    coolTime = skillCoolTimes[i],
                    centerEffect = skillEffects[i],
                    unitEffect = unitEffects[i]
                };
            }

            return array;
        }
    }
}