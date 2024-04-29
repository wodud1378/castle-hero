using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.System.Wave
{
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