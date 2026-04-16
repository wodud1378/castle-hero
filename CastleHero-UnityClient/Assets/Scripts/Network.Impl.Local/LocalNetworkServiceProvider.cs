using CastleHero.Network.Service;

namespace CastleHero.Network.Impl.Local
{
    /// <summary>
    /// 서버 없이 실행하기 위한 인메모리 네트워크 구현체 묶음.
    /// 기본값은 Backend 구현체이므로, CASTLEHERO_LOCAL_NETWORK 디파인이 활성화된 경우에만 사용된다.
    /// </summary>
    public class LocalNetworkServiceProvider : INetworkServiceProvider
    {
        public IUserService User { get; } = new LocalUserService();
        public IGameService Game { get; } = new LocalGameService();
        public ICharacterService Character { get; } = new LocalCharacterService();
        public IInventoryService Inventory { get; } = new LocalInventoryService();
        public IShopService Shop { get; } = new LocalShopService();
        public ISummonService Summon { get; } = new LocalSummonService();
        public ICastleService Castle { get; } = new LocalCastleService();
    }
}
    