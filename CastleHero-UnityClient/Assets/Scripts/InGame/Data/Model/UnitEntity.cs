using System;

namespace RGLabs.InGame.Data.Model
{
    [Serializable]
    public struct UnitEntity : IEntity
    {
        public int Id => id;
        
        public int id;
        public float size;
        public string prefab;
        public string name;
        public float hp;
        public float speed;
    }
}