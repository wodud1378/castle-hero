using System;
using UnityEngine;

namespace RGLabs.InGame.Data.Model
{
    [Serializable]
    public struct UnitEntity : IEntity
    {
        [field: SerializeField]
        public int Id { get; set; }
        public float size;
        public string prefab;
        public string name;
        public float hp;
        public float speed;
    }
}