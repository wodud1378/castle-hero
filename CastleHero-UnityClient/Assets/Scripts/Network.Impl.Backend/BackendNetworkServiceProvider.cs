using CastleHero.Network.Impl.Backend.Services;
using CastleHero.Network.Service;

namespace CastleHero.Network.Impl.Backend
{
    /// <summary>
    /// 뒤끝(TheBackend) SDK 기반 서비스 구현체를 묶어서 제공한다.
    /// </summary>
    public class BackendNetworkServiceProvider : INetworkServiceProvider
    {
        public IUserService User { get; } = new BackendUserService();
        public IGameService Game { get; } = new BackendGameService();
        public ICharacterService Character { get; } = new BackendCharacterService();
        public IInventoryService Inventory { get; } = new BackendInventoryService();
        public IShopService Shop { get; } = new BackendShopService();
        public ISummonService Summon { get; } = new BackendSummonService();
        public ICastleService Castle { get; } = new BackendCastleService();
    }
}
