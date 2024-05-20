using System;
using RGLabs.Data.DB;
using UnityEngine;

namespace RGLabs.Data.Model
{
    [Serializable]
    public struct StageEntity : IEntity
    {
        [DataField("Stage_index")]
        [field:SerializeField] public int Id { get; set; }

        [DataField("Stage_Rwd_Gold_Min")]
        public int goldMin;
        
        [DataField("Stage_Rwd_Gold_Max")]
        public int goldMax;

        [DataField("Stage_Rwd_Exp")]
        public int exp; 
        
        [DataField("Stage_Mob_Wave")]
        public int waveGroupId;

        [DataField("Stage_Rwd_Item_Per")] 
        public int itemPer;

        [DataField("Stage_Rwd_Item_ID")] 
        public int propItemId;

        [DataField("Stage_Rwd_Item_Value")]
        public int propItemQty;
    }
}