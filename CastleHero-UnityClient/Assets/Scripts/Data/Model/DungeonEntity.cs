using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data.DB;
using RGLabs.Network.Service;
using RGLabs.Prepare.UI;
using RGLabs.Utility;
using Random = UnityEngine.Random;

namespace RGLabs.Data.Model
{
    public enum DungeonType
    {
        Assault = 0,
        Escort,
        Raid,
        Invasion
    }
    
    public enum DungeonDetailType
    {
        None, Executor, Ground = 0,
        Slaughterer, Fire = 1,
        Punisher, Wind = 2,
        Transcendent, Water = 3,
    }

    public struct DungeonEntity : IGameEntity
    {
        public GameType Type => GameType.Dungeon;

        [DataField("Dg_index")] public int Id { get; set; }
        public bool IsValid { get; set; }

        public int Lv => lv;

        [DataField("Dg_Bg")] public string Map { get; set; }

        [DataField("Dg_Sound")] public string Bgm { get; set; }

        [DataField("Dg_Act")] public int Ap { get; set; }

        [DataField("Dg_Time")] public int TimeLimit { get; set; }

        [DataField("Dg_Mob_Wave")] public int WaveId { get; set; }

        [DataField("Dg_Rwd_Gold_Min")] public int MinGold { get; set; }

        [DataField("Dg_Rwd_Gold_Max")] public int MaxGold { get; set; }

        public int Exp => 0;

        [DataField("Dg_Castle")] public string castlePrefab;
        
        [DataField("Dg_Image")] public string image;

        [DataField("Dg_Name")] public string name;

        [DataField("Dg_Type")] public DungeonType type;

        [DataField("Dg_DetailType")] public DungeonDetailType detailType;

        [DataField("Dg_Week")] public int dayOfWeek;

        [DataField("Dg_Lv")] public int lv;

        [DataField("Dg_Rwd_Item_Grp_ID")] public int rewardGroup;

        public bool IsOpened()
        {
            var openDays = OpenDaysOfWeek();
            var dow = NetworkService.CurrentTimeByLocal().DayOfWeek;

            return openDays.Contains(dow);
        }

        public List<DayOfWeek> OpenDaysOfWeek()
        {
            var list = new List<DayOfWeek>();

            if (dayOfWeek == 0)
            {
                for (var dow = DayOfWeek.Sunday; dow <= DayOfWeek.Saturday; ++dow)
                {
                    list.Add(dow);
                }
            }
            else
            {
                var dow = (DayOfWeek)dayOfWeek;
                list.Add(dow);

                if (dow != DayOfWeek.Sunday)
                    list.Add(DayOfWeek.Sunday);

                if (dow != DayOfWeek.Saturday)
                    list.Add(DayOfWeek.Saturday);
            }

            return list;
        }

        public List<Reward> GetRewardsForDisplay()
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