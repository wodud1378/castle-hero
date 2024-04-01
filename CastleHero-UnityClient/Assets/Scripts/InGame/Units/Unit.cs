using RGLabs.InGame.Units.Interfaces;

namespace RGLabs.InGame.Units
{
    public class Unit
    {
        private Configuration _configuration;
        
        private IConfigHandler _configHandler;
        private ILifeCycle _lifeCycle;
        private IMovement _movement;
    }
}
