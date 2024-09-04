using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public struct ElementEntity : IEntity
    {
        [DataField("Element_Lv")]
        public int Id { get; set; }
        public bool IsValid { get; set; }

        public float forward;
        public float reverse;
    }
}