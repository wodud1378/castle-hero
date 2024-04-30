using System;
using UnityEngine;

namespace RGLabs.Data.Model
{
    [Serializable]
    public struct RewardEntity : IEntity
    {
        [field: SerializeField]
        public int Id { get; set; }

        public int itemId;
        public int minQuantity;
        public int maxQuantity;
    }
}