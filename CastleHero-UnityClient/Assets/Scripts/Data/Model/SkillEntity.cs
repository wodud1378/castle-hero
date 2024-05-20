using System;
using UnityEngine;

namespace RGLabs.Data.Model
{
    [Serializable]
    public struct SkillEntity : IEntity
    {
        [field:SerializeField] public int Id { get; set; }

        public string name;
        public float range;
        public float coolTime;
        
        public int[] stats;
        public float[] values;
        public int[] groups;
        
        public string desc;
    }
}