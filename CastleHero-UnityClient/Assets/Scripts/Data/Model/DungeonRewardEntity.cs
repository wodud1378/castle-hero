using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct DungeonRewardEntity : IEntity
    {
        [DataField("Dg_Rwd_Item_Grp_ID")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        public int[] itemIds;
        public int[] minQuantities;
        public int[] maxQuantities;
        public float[] probabilities;
    }
}