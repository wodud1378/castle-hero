using System;
using RGLabs.Data.DB;
using UnityEngine;

namespace RGLabs.Data.Model
{
    [Serializable]
    public struct UnitEntity : IEntity
    {
        [field: SerializeField]
        [DataField("Character_ID")]
        public int Id { get; set; }
        
        [DataField("Character_Name")]
        public string name;

        [DataField("Character_Icon")]
        public string icon;
        
        [DataField("Character_Prefab")]
        public string prefab;
        
        [DataField("Character_Skin")]
        public string skinName;
        
        [DataField("Character_Projectile")]
        public string projectile;

        [DataField("Character_Class")]
        public int grade;
        
        [DataField("Character_Team")]
        public int team;
        
        [DataField("Character_Position")]
        public int role;
        
        [DataField("Character_Atk_type")]
        public int atkLayer;
        
        [DataField("Character_Def_type")]
        public int defLayer;
        
        [DataField("Character_Order_atk")]
        public int atkOrder;
        
        [DataField("Character_Recovery")]
        public float recovery;
        
        [DataField("Character_Hp")]
        public float hp;
        
        [DataField("Character_Atk")]
        public float atk;
        
        [DataField("Character_Atk")]
        public float critical;
        
        [DataField("Character_Cri_Damage")]
        public float criticalAtk;
        
        [DataField("Character_Speed_Atk")]
        public float atkSpeed;
        
        [DataField("Character_Speed_Move")]
        public float speed;
        
        [DataField("Character_Range_Atk")]
        public float atkRange;
        
        [DataField("Character_Range_Move")]
        public float moveRange;
        
        public float size;
    }
}