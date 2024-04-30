using System;
using UnityEngine;

namespace RGLabs.Data.Model
{
    public enum Pattern
    {
        ForEach,
        AtOnce,
    }
    
    [Serializable]
    public struct SpawnInfo
    {
        public int id;
        public int count;
        public int lv;
        public int area;
        public float timeStep;
    }
    
    [Serializable]
    public struct WaveEntity : IEntity
    {
        [field:SerializeField]public int Id { get; set; }

        public int groupId;
        public float startTime;
        public Pattern pattern;
        public SpawnInfo[] info;
    }
}