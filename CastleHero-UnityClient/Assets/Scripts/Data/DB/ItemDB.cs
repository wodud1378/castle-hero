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
    }
}