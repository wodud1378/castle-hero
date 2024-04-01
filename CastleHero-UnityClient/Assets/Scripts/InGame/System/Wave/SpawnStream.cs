using System.Collections.Generic;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.System.Wave
{
    [CreateAssetMenu(fileName = "SpawnStream", menuName = "Scriptable Object/SpawnStream")]
    public class SpawnStream : ScriptableObject
    {
        private Queue<MonsterEntity> _queue = new Queue<MonsterEntity>();

        public int Count => _queue.Count;
        
        public void Enqueue(MonsterEntity entity) => _queue.Enqueue(entity);

        public MonsterEntity Dequeue() => _queue.Dequeue();
    }
}