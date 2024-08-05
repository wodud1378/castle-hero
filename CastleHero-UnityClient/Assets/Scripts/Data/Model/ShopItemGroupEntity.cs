using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct ShopItemGroupEntity : IEntity
    {
        [DataField("Shop_Rwd_Grp_ID")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Shop_Rwd_Grp_Type")]
        public int[] ids;
        [DataField("Shop_Rwd_Grp_Value")]
        public int[] quantities;
    }
}