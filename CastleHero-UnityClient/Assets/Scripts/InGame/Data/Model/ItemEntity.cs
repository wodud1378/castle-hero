using System;

namespace RGLabs.InGame.Data.Model
{
    [Serializable]
    public struct ItemEntity : IEntity
    {
        public int Id => id;

        public int id;
    }
}