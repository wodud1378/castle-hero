using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.Data.DB
{
    [CreateAssetMenu(fileName = "Items", menuName = "Scriptable Object/Items")]
    public class ItemDB : DB<ItemEntity>
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