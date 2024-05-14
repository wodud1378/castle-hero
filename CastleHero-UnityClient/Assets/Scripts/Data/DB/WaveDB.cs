using System;
using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Data.DB
{
    [CreateAssetMenu(fileName = "Waves", menuName = "Scriptable Object/Waves")]
    public class WaveDB : DB<WaveEntity>, IDataBase
    {
        protected override WaveEntity FallBackEntity() =>
            new()
            {
                Id = -1,
                startTime = -1,
            };

        public WaveEntity[] Map(int groupId) => Array.FindAll(_entities, (x) => x.groupId == groupId);
        
        public void Load(object[] data)
        {
            int length = data.Length;
            _entities = new WaveEntity[length];

            for (int i = 0; i < length; ++i)
            {
                _entities[i] = (WaveEntity)data[i];
            }
        }
    }
}