using Cysharp.Threading.Tasks;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Service
{
    public interface ICharacterService
    {
        UniTask<Result<UnitGrowth>> Growth(GrowthAction action, int unitId, int itemId, int quantity);
        UniTask<Result<UnitInfo>> Equip(int unitId, string guid);
        UniTask<Result<UnitInfo>> Release(int unitId, string guid);
    }
}
