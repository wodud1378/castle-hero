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
    }
}