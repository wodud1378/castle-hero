using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct UnitRateEntity :IEntity
    {
        [DataField("Rate_Lv")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Rate_Soul")]
        public int soul;
        
        [DataField("Rate_Gold_Normal")]
        public int gold;
    }
}