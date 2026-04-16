using CastleHero.Data.DB;

namespace CastleHero.Data.Model
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
        
        [DataField("Rate_Gold_Rare")]
        public int goldForRare;
        
        [DataField("Rate_Gold_Epic")]
        public int goldForEpic;
        
        [DataField("Rate_Gold_Legend")]
        public int goldForLegend;
    }
}