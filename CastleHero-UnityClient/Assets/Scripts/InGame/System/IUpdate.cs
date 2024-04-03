namespace RGLabs.InGame.System
{
    public interface IUpdate
    {
        public void Init();
        
        public void ProcessUpdate(float deltaTime);
    }
}