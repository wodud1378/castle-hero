using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Data.DB
{
    [CreateAssetMenu(fileName = "Stages", menuName = "Scriptable Object/Stages")]
    public class StageDB : DB<StageEntity>, IDataBase
    {
        protected override StageEntity FallBackEntity() => new();
        
        public void Load(object[] data)
        {
            int length = data.Length;
            _entities = new StageEntity[length];

            for (int i = 0; i < length; ++i)
            {
                _entities[i] = (StageEntity)data[i];
            }
        }
    }
}