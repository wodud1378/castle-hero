using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Data.DB
{
    [CreateAssetMenu(fileName = "Units", menuName = "Scriptable Object/Units")]
    public class UnitDB : DB<UnitEntity>, IDataBase
    {
        protected override UnitEntity FallBackEntity() =>
            new()
            {
                Id = -1,
                prefab = string.Empty,
                name = string.Empty,
                hp = 0,
                speed = 0,
            };
        
        public void Load(object[] data)
        {
            int length = data.Length;
            _entities = new UnitEntity[length];

            for (int i = 0; i < length; ++i)
            {
                _entities[i] = (UnitEntity)data[i];
            }
        }
    }
}