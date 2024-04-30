using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Data.DB
{
    [CreateAssetMenu(fileName = "Units", menuName = "Scriptable Object/Units")]
    public class UnitDB : DB<UnitEntity>
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
    }
}