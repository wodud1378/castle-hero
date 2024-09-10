using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct SkillEntity : IEntity
    {
        [DataField("Skill_Id")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }

        [DataField("Name")]
        public string name;

        [DataField("Skill_Sound")] 
        public string sfx;
        
        [DataField("CoolTime")]
        public float coolTime;
        
        [DataField("Scale_x")]
        public float x;

        [DataField("Scale_y")] 
        public float y;
        
        [DataField("Duration", 3)]
        public float duration;

        [DataField("Stack", 4)]
        public int stack;
      
        [DataField("Status", 0)]
        public int[] stats;
        
        [DataField("Type", 0)]
        public float[] values;

        [DataField("Target")] 
        public int[] targetQty;
        
        [DataField("Grp", 5)]
        public int[] groups;

        [DataField("Effect")] 
        public string[] effects;
        
        [DataField("Description")]
        public string desc;
        
        [DataField("Range")]
        public string ignore;
    }
}