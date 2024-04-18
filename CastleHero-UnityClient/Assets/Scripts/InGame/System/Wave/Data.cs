using System;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.System.Wave
{
    public enum Pattern
    {
        ForEach,
        AtOnce,
    }

    [Serializable]
    public struct WaveGroup
    {
        public int id;
        public float startTime;
        public Pattern pattern;
        public SpawnInfo[] info;
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

    public struct SpawnEvent
    {
        public int id;
        public int lv;
        public int area;

        public void Set(int id, int lv, int area)
        {
            this.id = id;
            this.lv = lv;
            this.area = area;
        }
    }
    
    public struct UnitCreation
    {
        public Vector2 position;
        public UnitEntity entity;
    }

    public struct ReleaseEvent
    {
        public UnitBehaviour unit;
    }
}