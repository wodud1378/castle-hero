using RGLabs.InGame.Units.Interfaces;

namespace RGLabs.InGame.Units
{
    public class Configuration
    {
        public float Hp { get; private set; }
        public float Speed { get; private set; }

        public bool Invincible { get; set; } = false;
        public bool Freeze { get; set; } = false;

        public Configuration(float hp, float speed)
        {
            Hp = hp;
            Speed = speed;
        }
    }
}
