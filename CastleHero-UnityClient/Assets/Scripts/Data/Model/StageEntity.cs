using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct StageEntity : IGameEntity
    {
        public GameType Type => GameType.Stage;
        
        [DataField("Stage_index")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }
        
        [DataField("Stage_Bg")] 
        public string Map { get; set; }
        
        [DataField("Stage_Act")] 
        public int Ap { get; set; }

        [DataField("Stage_Time")]
        public int TimeLimit { get; set; }
        
        [DataField("Stage_Mob_Wave")]
        public int WaveId { get; set; }
        
        [DataField("Stage_Rwd_Gold_Min")]
        public int MinGold { get; set; }
        
        [DataField("Stage_Rwd_Gold_Max")]
        public int MaxGold { get; set; }

        [DataField("Stage_Rwd_Exp")]
        public int exp; 

        [DataField("Stage_Rwd_Item_Per")] 
        public int itemPer;

        [DataField("Stage_Rwd_Item_ID")] 
        public int propItemId;

        [DataField("Stage_Rwd_Item_Value")]
        public int propItemQty;
    }
}