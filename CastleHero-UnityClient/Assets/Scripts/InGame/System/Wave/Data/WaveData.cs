using System;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.System.Wave.Data
{
    public enum SpawnAt
    {
        ForEach,
        AtOnce
    }
    
    [Serializable]
    public struct WaveData
    {
        public float timeStep;
        public float start;
        public float end;

        public SpawnInfoData[] info;
    }
    
    [Serializable]
    public struct SpawnInfoData
    {
        public int area;
        public SpawnAt spawnAt;
        public SpawnDetailData[] details;
    }
    
    [Serializable]
    public struct SpawnDetailData
    {
        public int id;
        public int count;
    }

    [Serializable]
    public struct SpawnEventData
    {
        public int area;
        public SpawnAt spawnAt;
        public UnitEntity[] entities;
    }
}