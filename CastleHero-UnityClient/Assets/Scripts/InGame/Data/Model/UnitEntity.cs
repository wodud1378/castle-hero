using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace RGLabs.InGame.Data.Model
{
    [Serializable]
    public struct UnitEntity : IEntity
    {
        [field: SerializeField]
        public int Id { get; set; }
        public string name;
        public string prefab;
        public string skinName;
        
        public float size;
        public float hp;
        public float speed;
        public float moveRange;
        public float atk;
        public float atkRange;
        public float atkSpeed;
        public float critical;
        public float criticalAtk;
    }
}