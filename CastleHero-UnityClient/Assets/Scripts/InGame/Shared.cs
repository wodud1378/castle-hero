using System;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.System.Wave;
using RGLabs.InGame.System.Wave.Data;
using UnityEngine;

namespace RGLabs.InGame
{
    [CreateAssetMenu(fileName = "Shared", menuName = "Scriptable Object/Shared")]
    public class Shared : ScriptableObject
    {
        public delegate void SpawnEvent(int id, int area, float[] spaces);

        public readonly DataStream<SpawnEventData> spawnDataStream = new();

        public event SpawnEvent OnSpawnEventRegistered;

        public void NotifySpawnEventRegistered(int id, int area, float[] spaces) => OnSpawnEventRegistered?.Invoke(id, area, spaces);
        
        public MonsterDB monsterDB = new();
    }
}