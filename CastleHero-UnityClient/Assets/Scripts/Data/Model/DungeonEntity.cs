using System;
using System.Collections.Generic;
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
        public int dayOfWeek;
        
        [DataField("Dg_Lv")]
        public int lv;
        
        [DataField("Dg_Rwd_Item_Grp_ID")]
        public int rewardGroup;
        
        public List<DayOfWeek> OpenDaysOfWeek()
        {
            var list = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday };

            if (dayOfWeek == 0)
            {
                for (var dow = DayOfWeek.Monday; dow <= DayOfWeek.Friday; ++dow)
                {
                    list.Add(dow);
                }
            }
            else
                list.Add((DayOfWeek)dayOfWeek);

            return list;
        }

        public string OpenDaysOfWeekText()
        {
            if (dayOfWeek == 0)
                return "매일";
       
            var dow = (DayOfWeek)dayOfWeek;
            int id = dow switch
            {
                DayOfWeek.Sunday => 570,
                _ => 563 + (int)dow
            };

            return Storage.localize.Get(id);
        }
    }
}