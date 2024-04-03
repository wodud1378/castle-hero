using System;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.System.Wave
{
    public enum SpawnAt
    {
        ForEach,
        AtOnce
    }
    
    [Serializable]
    public struct Wave
    {
        public float timeStep;
        public float start;
        public float end;

        public SpawnInfo[] info;
    }
    
    [Serializable]
    public struct SpawnInfo
    {
        public int area;
        public SpawnAt spawnAt;
        public SpawnDetail[] details;
    }
    
    [Serializable]
    public struct SpawnDetail
    {
        public int id;
        public int count;
    }

    public struct SpawnEvent
    {
        public int area;
        public SpawnAt spawnAt;
        public UnitEntity[] entities;
    }
    
    public struct CreationRequest
    {
        public Vector2 position;
        public UnitEntity entity;
    }
}