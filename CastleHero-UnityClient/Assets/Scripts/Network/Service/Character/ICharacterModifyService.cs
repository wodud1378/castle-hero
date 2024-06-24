using Cysharp.Threading.Tasks;
using RGLabs.Network.Model;

namespace RGLabs.Network.Service.Character
{
    public interface ICharacterModifyService
    {
        public UniTask<LevelUpResult> LevelUp(UnitInfo unit, ConsumableItem item, int consumeQuantity);
        public UniTask<UpgradeResult> Upgrade(UnitInfo unit, ConsumableItem item, int consumeQuantity);
    }
}