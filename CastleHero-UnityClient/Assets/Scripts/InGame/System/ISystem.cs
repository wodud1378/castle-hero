namespace RGLabs.InGame.System
{
    public interface ISystem
    {
        public void Init();
        
        public void ProcessUpdate(float deltaTime);
    }
}