using System.Collections.Generic;
using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Data.DB
{
    [CreateAssetMenu(fileName = "Units", menuName = "Scriptable Object/Units")]
    public class UnitDB : DB<UnitEntity>, IDataBase
    {
        public readonly Dictionary<int, float> sizeCache = new();
        
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

        public void CacheUnitSizes()
        {
            foreach (var entity in _entities)
            {
                sizeCache.TryAdd(entity.Id, entity.size);
            }
        }
    }
}