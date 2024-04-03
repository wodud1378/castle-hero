using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Units.Interfaces;

namespace RGLabs.InGame.Units.Impl
{
    public class SingleTargetAttack : IAttack
    {
        public GameUnit[] Targets { get; } = new GameUnit[1];

        public void RegisterTarget(GameUnit unit)
        {
            Targets[0] = unit;
        }

        // TODO : 추 후 애니메이션 공격 타이밍에 이벤트 등록
        public void Action()
        {
            
        }
    }
}