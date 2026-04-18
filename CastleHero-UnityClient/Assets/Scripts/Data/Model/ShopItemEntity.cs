using System;
using CastleHero.Data.DB;

namespace CastleHero.Data.Model
{
    public enum ShopCategory
    {
        NoAds,
        Contract,
        BattlePass,
        Package,
        UnitPackage,
        Currency01,
        Currency02,
        Currency03,
        Supply,
        Limited
    }
    
    public enum PaymentType
    {
        Default = 0,
        Ad,
        Free,
    }
    
    public struct ShopItemEntity : IEntity
    {
        [DataField("Shop_ID")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Shop_Name")]
        public string name;
        [DataField("Shop_Prefab")]
        public string prefab;
        [DataField("Shop_Descripsion")]
        public string desc;
        [DataField("Shop_Type")]
        public ShopCategory category;
        [DataField("Shop_Rwd_Grp")]
        public int groupId;
        [DataField("Shop_Duration")]
        public int duration;
        [DataField("Shop_index")]
        public int order;
        [DataField("Shop_InApp")] // TODO: 실제 컬럼명 확인 필요
        public string inApp;
        [DataField("Shop_Cost_Type")]
        public int costId;
        [DataField("Shop_Cost_Value")]
        public int costValue;
        [DataField("Shop_Count_Free")]
        public int countForFree;
        [DataField("Shop_Count_Ad")]
        public int countForAd;
        [DataField("Shop_Count")]
        public int totalCount;
        [DataField("Shop_Count_Reset")]
        public int resetDays;
        [DataField("Shop_Link_Front" )]
        public int prevItem;
        [DataField("Shop_Link_Next")]
        public int nextItem;
        [DataField("Shop_Day_Start")]
        public DateTime startDate;
        [DataField("Shop_Day_End")]
        public DateTime endDate;

    }
}