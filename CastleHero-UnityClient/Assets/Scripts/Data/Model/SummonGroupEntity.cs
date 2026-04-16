using CastleHero.Data.DB;

namespace CastleHero.Data.Model
{
    public struct SummonGroupEntity : IEntity
    {
        [DataField("Summon_Grp_Index")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Summon_Grp_ID")]
        public int groupId;
        [DataField("Summon_Grp_Per")]
        public float weight;

        [DataField("Summon_Grp_Character_ID")]
        public int unitId;
        [DataField("Item_Parts_ID")]
        public int soulId;
        [DataField("Parts_Value")]
        public int soulCount;
    }
}