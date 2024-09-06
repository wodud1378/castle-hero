using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct ElementEntity : IEntity
    {
        [DataField("Lv")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        [DataField("Forward")]
        public float forward;
        [DataField("Reverse")]
        public float reverse;
    }
}