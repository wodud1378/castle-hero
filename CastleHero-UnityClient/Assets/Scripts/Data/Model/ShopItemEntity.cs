using System;
using CastleHero.Data.DB;

using CastleHero.Common.Pattern;
using CastleHero.Common.Localize;
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
        [DataField("")] 
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

        public string CategoryText()
        {
            int id = category switch
            {
                ShopCategory.NoAds => 112,
                ShopCategory.Contract => 118,
                ShopCategory.BattlePass => 122,
                ShopCategory.Package => 125,
                ShopCategory.UnitPackage => 132,
                ShopCategory.Currency01 => 134,
                ShopCategory.Currency02 => 134,
                ShopCategory.Currency03 => 134,
                ShopCategory.Supply => 138,
                ShopCategory.Limited => 0,
                _ => 0
            };
            
            return ServiceLocator.Get<LocalizeText>().Get(id);
        }
    }
}