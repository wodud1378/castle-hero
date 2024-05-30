using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct UnitLevelEntity : IEntity
    {
        [DataField("Lv")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Exp")] 
        public int exp;
        
        [DataField("Gold")]
        public int gold;
    }
}