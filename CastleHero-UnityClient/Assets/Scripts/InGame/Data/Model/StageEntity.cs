using System;
using UnityEngine;

namespace RGLabs.InGame.Data.Model
{
    [Serializable]
    public struct StageEntity : IEntity
    {
        [field:SerializeField] 
        public int Id { get; set; }

        public int waveGroupId;
        public int[] rewards;
    }
}