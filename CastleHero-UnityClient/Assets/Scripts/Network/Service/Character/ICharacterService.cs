using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Model;

namespace RGLabs.Network.Service.Character
{
    public interface ICharacterService : INetworkService
    {
        public UniTask<LevelUpResult> LevelUp(UnitInfo unit, ConsumableItem item, int consumeQuantity);
        public UniTask<UpgradeResult> Upgrade(UnitInfo unit, ConsumableItem item, int consumeQuantity);

        public UniTask<FieldCharacter[]> LoadFormation();
        public UniTask SaveFormation(IEnumerable<FieldCharacter> characters);
    }
}