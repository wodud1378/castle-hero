using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
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

        [DataField("Castle_Skill_Value")] 
        public float[] skillValues;
    }
}