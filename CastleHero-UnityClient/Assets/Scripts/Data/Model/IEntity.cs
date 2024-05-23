namespace RGLabs.Data.Model
{
    public interface IEntity
    {
        public int Id { get; set; }
        public bool IsValid { get; set; }
    }
}