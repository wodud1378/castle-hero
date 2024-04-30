using System;
using UnityEngine;

namespace RGLabs.Data.Model
{
    [Serializable]
    public struct ItemEntity : IEntity
    {
        [field: SerializeField]
        public int Id { get; set; }

        public string icon;
        public string name;
        public string desc;
    }
}