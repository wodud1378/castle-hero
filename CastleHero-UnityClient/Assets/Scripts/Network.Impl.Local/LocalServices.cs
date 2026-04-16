using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Common.InApp;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Impl.Local
{
    // TODO: 진짜 로컬/인메모리 구현 작성. 현재는 서버 없이 실행하는 환경을 위한 스텁.
    //       Phase H(Storage 라이프사이클 정리)와 함께 진행.

    public class LocalUserService : IUserService
    {
        public UniTask SaveFormation(FormationDto formation) => throw NotImpl();
        public UniTask<Result> UpdateStamina() => throw NotImpl();
        public UniTask<Result> UpdateNickname(string nickname) => throw NotImpl();
        public UniTask<Result<UserDataDto>> GetUserData() => throw NotImpl();
        public UniTask<Result<UserDataDto>> CreateUserData() => throw NotImpl();

        private static NotImplementedException NotImpl() =>
            new("LocalUserService 는 아직 구현되지 않았습니다. CASTLEHERO_LOCAL_NETWORK 사용 시 구현 필요.");
    }

    public class LocalGameService : IGameService
    {
        public UniTask<Result<List<int>>> GetOpenedDungeonLayers() => throw NotImpl();
        public UniTask<Result> Start(GameType type, int id) => throw NotImpl();
        public UniTask<Result<GameCleared>> Clear(GameType type, int id) => throw NotImpl();

        private static NotImplementedException NotImpl() =>
            new("LocalGameService 는 아직 구현되지 않았습니다.");
    }

    public class LocalCharacterService : ICharacterService
    {
        public UniTask<Result<UnitGrowth>> Growth(GrowthAction action, int unitId, int itemId, int quantity) => throw NotImpl();
        public UniTask<Result<UnitInfo>> Equip(int unitId, string guid) => throw NotImpl();
        public UniTask<Result<UnitInfo>> Release(int unitId, string guid) => throw NotImpl();

        private static NotImplementedException NotImpl() =>
            new("LocalCharacterService 는 아직 구현되지 않았습니다.");
    }

    public class LocalInventoryService : IInventoryService
    {
        public UniTask<Result<OpenBox>> OpenChest(int chestId, int quantity) => throw NotImpl();
        public UniTask<Result<int>> Sell(IItem[] items, int[] quantities) => throw NotImpl();
        public UniTask<Result<List<IItem>>> Combine(int id, int quantity) => throw NotImpl();
        public UniTask<Result<StaminaDto>> AddStamina(int id, int amount) => throw NotImpl();
        public UniTask<Result<EquipItem>> Refine(string guid, int itemId) => throw NotImpl();

        private static NotImplementedException NotImpl() =>
            new("LocalInventoryService 는 아직 구현되지 않았습니다.");
    }

    public class LocalShopService : IShopService
    {
        public void RegisterIAP(IAPManager iap) { }
        public string InAppPrice(string productKey, int fallBack = -1) => $"\uffe6 {fallBack:N0}";
        public UniTask<Result> RefreshProducts() => throw NotImpl();
        public UniTask<Result<Pack>> ReceiveSubscribedItems() => throw NotImpl();
        public UniTask<Result<ItemBought>> BuyItem(PaymentType type, int id) => throw NotImpl();

        private static NotImplementedException NotImpl() =>
            new("LocalShopService 는 아직 구현되지 않았습니다.");
    }

    public class LocalSummonService : ISummonService
    {
        public UniTask<Result<Summon>> SummonOnce(int eventId, int costIndex) => throw NotImpl();
        public UniTask<Result<Summon>> SummonTenth(int eventId, int costIndex) => throw NotImpl();

        private static NotImplementedException NotImpl() =>
            new("LocalSummonService 는 아직 구현되지 않았습니다.");
    }

    public class LocalCastleService : ICastleService
    {
        public UniTask<Result<CastleGrowth>> LvUp() => throw NotImpl();

        private static NotImplementedException NotImpl() =>
            new("LocalCastleService 는 아직 구현되지 않았습니다.");
    }
}
