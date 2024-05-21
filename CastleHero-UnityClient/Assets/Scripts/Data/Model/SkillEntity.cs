using System;
using RGLabs.Data.DB;
using UnityEngine;

namespace RGLabs.Data.Model
{
    [Serializable]
    public struct SkillEntity : IEntity
    {
        [DataField("Id")]
        [field:SerializeField] public int Id { get; set; }

        [DataField("Name")]
        public string name;
        
        [DataField("CoolTime")]
        public float coolTime;
        
        [DataField("Range")]
        public float range;

        [DataField("Duration", 3)]
        public float duration;

        [DataField("Stack", 4)]
        public int stack;
      
        [DataField("Status")]
        public int[] stats;
        
        [DataField("Type")]
        public float[] values;

        [DataField("Target")] 
        public int[] targetQty;
        
        [DataField("Grp")]
        public int[] groups;
        
        [DataField("Description")]
        public string desc;
    }
}