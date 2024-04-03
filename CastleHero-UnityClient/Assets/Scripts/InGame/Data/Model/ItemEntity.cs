using System;
using UnityEngine;

namespace RGLabs.InGame.Data.Model
{
    [Serializable]
    public struct ItemEntity : IEntity
    {
        [field: SerializeField]
        public int Id { get; set; }
    }
}