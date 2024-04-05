using System;
using RGLabs.InGame.Data.Model;

namespace RGLabs.InGame.Data.DB
{
    [Serializable]
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