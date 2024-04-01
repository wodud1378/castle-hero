namespace RGLabs.InGame.Units.Interfaces
{
    public interface ILifeCycle
    {
        public void OnCreate();
        public void OnAlive();
        public void OnDead();
        public void OnDestroy();
    }
}
