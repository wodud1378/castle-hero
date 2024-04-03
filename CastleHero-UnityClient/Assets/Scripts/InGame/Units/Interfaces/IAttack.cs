using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Behaviours.Unit;

namespace RGLabs.InGame.Units.Interfaces
{
    public interface IAttack
    {
        public GameUnit[] Targets { get; }
        
        public void RegisterTarget(GameUnit unit);
        public void Action();
    }
}