using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Data.DB
{
    [CreateAssetMenu(fileName = "Items", menuName = "Scriptable Object/Items")]
    public class ItemDB : DB<ItemEntity>, IDataBase
    {
        protected override ItemEntity FallBackEntity() =>
            new()
            {
                Id = -1,
                icon = string.Empty,
                name = string.Empty,
                desc = string.Empty
            };

        public void Load(object[] data)
        {
            int length = data.Length;
            _entities = new ItemEntity[length];

            for (int i = 0; i < length; ++i)
            {
                _entities[i] = (ItemEntity)data[i];
            }
        }
    }
}