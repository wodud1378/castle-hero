using System;
using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct ShopItemEntity : IEntity
    {
        [DataField("Shop_ID")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Shop_Name")]
        public string name;
        [DataField("Shop_Image")]
        public string image;
        [DataField("Shop_Description")]
        public string desc;
        [DataField("Shop_Type")]
        public int category;
        [DataField("Shop_Rwd_Grp")]
        public int groupId;
        [DataField("Shop_Index")]
        public int order;
        [DataField("Shop_Cost_Type")]
        public int coastId;
        [DataField("Shop_Cost_Value")]
        public int coastValue;
        [DataField("Shop_Count_Free")]
        public int countForFree;
        [DataField("Shop_Count_Ad")]
        public int countForAd;
        [DataField("Shop_Count")]
        public int count;
        [DataField("Shop_Count_Reset")]
        public int resetDays;
        [DataField("Shop_Link_Front" )]
        public int prevItem;
        [DataField("Shop_Link_Next")]
        public int nextItem;
        [DataField("Shop_Day_Start")]
        public DateTime startDate;
        [DataField("Shop_Day_End")]
        public DateTime expireDate;
    }
}