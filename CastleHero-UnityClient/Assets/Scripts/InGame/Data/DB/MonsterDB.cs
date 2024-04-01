using System;
using RGLabs.InGame.Data.Model;

namespace RGLabs.InGame.Data.DB
{
    [Serializable]
    public class MonsterDB : DB<MonsterEntity>
    {
        protected override MonsterEntity FallBackEntity() =>
            new()
            {
                id = -1,
                prefab = string.Empty,
                name = string.Empty,
                hp = 0,
                speed = 0,
            };
    }
}