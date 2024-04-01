namespace RGLabs.InGame.Units.Interfaces
{
    public interface IConfigHandler
    {
        public void OnInvincibilityChanged(bool isInvincible);
        public void OnFreeze(bool freeze);
    }
}