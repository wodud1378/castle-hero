using System;

namespace RGLabs.InGame.Data.Model
{
    [Serializable]
    public struct MonsterEntity : IEntity
    {
        public int Id => id;
        
        public int id;
        public string prefab;
        public string name;
        public float hp;
        public float speed;
    }
}