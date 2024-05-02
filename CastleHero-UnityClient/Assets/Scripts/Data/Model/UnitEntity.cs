using System;
using UnityEngine;

namespace RGLabs.Data.Model
{
    [Serializable]
    public struct UnitEntity : IEntity
    {
        [field: SerializeField]
        public int Id { get; set; }
        public string name;

        public string icon;
        public string prefab;
        public string skinName;
        public string projectile;

        public int grade;
        public int role;
        public int team;
        public int atkOrder;
        public int atkLayer;
        public int defLayer;
        
        public float size;
        public float hp;
        public float speed;
        public float moveRange;
        public float atk;
        public float atkRange;
        public float atkSpeed;
        public float critical;
        public float criticalAtk;
        public float recovery;
    }
}