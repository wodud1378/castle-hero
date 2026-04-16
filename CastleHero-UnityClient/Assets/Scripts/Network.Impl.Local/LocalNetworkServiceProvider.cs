using CastleHero.Network.Impl.Local.Services;
using CastleHero.Network.Service;

namespace CastleHero.Network.Impl.Local
{
    /// <summary>
    /// 서버 없이 실행하기 위한 PlayerPrefs 기반 네트워크 구현체 묶음.
    /// 기본값은 Backend 구현체이므로, CASTLEHERO_LOCAL_NETWORK 디파인이 활성화된 경우에만 사용된다.
    /// </summary>
    public class LocalNetworkServiceProvider : INetworkServiceProvider
    {
        public IUserService User { get; }
        public IGameService Game { get; }
        public ICharacterService Character { get; }
        public IInventoryService Inventory { get; }
        public IShopService Shop { get; }
        public ISummonService Summon { get; }
        public ICastleService Castle { get; }

        public LocalNetworkServiceProvider(LocalUserDataStore store)
        {
            User = new LocalUserService(store);
            Game = new LocalGameService(store);
            Character = new LocalCharacterService(store);
            Inventory = new LocalInventoryService(store);
            Shop = new LocalShopService(store);
            Summon = new LocalSummonService(store);
            Castle = new LocalCastleService(store);
        }
    }
}
