using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct SummonEntity : IEntity
    {
        [DataField("Summon_index")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Summon_Bg")] 
        public string bg;

        [DataField("Summon_Comment")]
        public string comment;

        [DataField("Summon_Cost_Type")]
        public int[] item;

        [DataField("Summon_Cost_Once")]
        public int[] valuePerOnce;

        [DataField("Summon_Cost_Tenth")]
        public int[] valuePerTenth;

        [DataField("Summon_Grp_ID")] 
        public int groupId;
    }
}