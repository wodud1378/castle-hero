using System;
using System.Collections.Generic;
using RGLabs.Data.DB;
using RGLabs.Network.Service;
using RGLabs.Stage.UI;
using RGLabs.Utility;

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

        public int Lv => lv;

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

        public int Exp => 0;
        
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

        public bool IsOpened()
        {
            var openDays = OpenDaysOfWeek();
            var dow = NetworkService.CurrentTime().DayOfWeek;

            return openDays.Contains(dow);
        }
        
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

        public List<Reward> GetRewardItems()
        {
            var rewards = new List<Reward>();
            if (!Storage.db.dungeonRewards.TryFind(rewardGroup, out var group))
                return rewards;

            int index = 0;
            while (index.IsValidIndex(
                       group.itemIds,
                       group.probabilities,
                       group.minQuantities, 
                       group.maxQuantities))
            {
                if (Storage.db.items.TryFind(group.itemIds[index], out var itemEntity))
                {
                    rewards.Add(new()
                    {
                        icon = itemEntity.icon,
                        id = itemEntity.Id,
                        min = group.minQuantities[index],
                        max = group.maxQuantities[index],
                        percent = group.probabilities[index]
                    });
                }

                ++index;
            }

            return rewards;
        }
    }
}