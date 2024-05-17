namespace RGLabs.Data.Model
{
    public struct UnitLevelEntity : IEntity
    {
        public int Id { get; set; }

        public float hp;
        public float atk;
        public float critical;
        public float criticalAtk;
        public float speed;
        public float atkSpeed;
        public float atkRange;
        public float moveRange;
    }
}