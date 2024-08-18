using System.Collections.Generic;
using RGLabs.Data.DB;
using RGLabs.Stage.UI;

namespace RGLabs.Data.Model
{
    public struct StageEntity : IGameEntity
    {
        public GameType Type => GameType.Stage;
        
        [DataField("Stage_index")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }

        public int Lv => Id;
        
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

        public int Exp => exp;

        [DataField("Stage_Rwd_Exp")]
        public int exp; 

        [DataField("Stage_Rwd_Item_Per")] 
        public int itemPer;

        [DataField("Stage_Rwd_Item_ID")] 
        public int propItemId;

        [DataField("Stage_Rwd_Item_Value")]
        public int propItemQty;

        public List<Reward> GetRewardItems()
        {
            var rewards = new List<Reward>();

            if (Storage.db.items.TryFind(propItemId, out var itemEntity))
            {
                rewards.Add(new()
                {
                    icon = itemEntity.icon,
                    id = itemEntity.Id,
                    min = propItemQty,
                    max = propItemQty,
                    percent = itemPer
                });
            }

            return rewards;
        }
    }
}