namespace RGLabs.InGame.System
{
    public interface IUpdateLoop
    {
        public void Init();
        
        public void ProcessUpdate(float deltaTime);
    }
}