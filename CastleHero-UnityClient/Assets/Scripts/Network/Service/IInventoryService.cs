using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Service
{
    public interface IInventoryService
    {
        UniTask<Result<OpenBox>> OpenChest(int chestId, int quantity);
        UniTask<Result<int>> Sell(IItem[] items, int[] quantities);
        UniTask<Result<List<IItem>>> Combine(int id, int quantity);
        UniTask<Result<StaminaDto>> AddStamina(int id, int amount);
        UniTask<Result<EquipItem>> Refine(string guid, int itemId);
    }
}
