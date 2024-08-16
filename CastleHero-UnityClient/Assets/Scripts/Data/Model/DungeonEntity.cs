using System;
using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public enum DungeonType
    {
        Assault = 0,
        Escort,
        Raid,
        Invasion
    }
    
    public struct DungeonEntity : IGameEntity
    {
        public GameType Type => GameType.Dungeon;
        
        [DataField("Dg_index")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Dg_Bg")] 
        public string Map { get; set; }
        
        [DataField("Dg_Act")] 
        public int Ap { get; set; }

        [DataField("Dg_Time")]
        public int TimeLimit { get; set; }
        
        [DataField("Dg_Mob_Wave")]
        public int WaveId { get; set; }
        
        [DataField("Dg_Rwd_Gold_Min")]
        public int MinGold { get; set; }
        
        [DataField("Dg_Rwd_Gold_Max")]
        public int MaxGold { get; set; }
        
        [DataField("Dg_Image")]
        public string image;
        
        [DataField("Dg_Name")]
        public string name;
        
        [DataField("Dg_Type")]
        public DungeonType type;
        
        [DataField("Dg_Week")]
        public DayOfWeek dayOfWeek;
        
        [DataField("Dg_Lv")]
        public int lv;
        
        [DataField("Dg_Rwd_Item_Grp_ID")]
        public int rewardGroup;
    }
}