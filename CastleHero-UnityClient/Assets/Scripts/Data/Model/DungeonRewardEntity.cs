using CastleHero.Data.DB;

namespace CastleHero.Data.Model
{
    public struct DungeonRewardEntity : IEntity
    {
        [DataField("Dg_Rwd_Item_Grp_ID")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Dg_Rwd_Item_ID")]
        public int[] itemIds;
        
        [DataField("Dg_Rwd_Item_Value_Min")]
        public int[] minQuantities;
        
        [DataField("Dg_Rwd_Item_Value_Max")]
        public int[] maxQuantities;
        
        [DataField("Dg_Rwd_Item_Per")]
        public float[] probabilities;
    }
}