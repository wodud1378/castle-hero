using CastleHero.Common.Pattern;
using CastleHero.Network.Impl.Backend.Services;
using CastleHero.Network.Service;

namespace CastleHero.Network.Impl.Backend
{
    /// <summary>
    /// 뒤끝(TheBackend) SDK 기반 서비스 구현체를 묶어서 제공한다.
    /// </summary>
    public class BackendNetworkServiceProvider : INetworkServiceProvider
    {
        public IUserService User { get; }
        public IGameService Game { get; }
        public ICharacterService Character { get; }
        public IInventoryService Inventory { get; }
        public IShopService Shop { get; }
        public ISummonService Summon { get; }
        public ICastleService Castle { get; }

        public BackendNetworkServiceProvider(IServiceLocator sl)
        {
            User = new BackendUserService(sl);
            Game = new BackendGameService(sl);
            Character = new BackendCharacterService(sl);
            Inventory = new BackendInventoryService(sl);
            Shop = new BackendShopService(sl);
            Summon = new BackendSummonService(sl);
            Castle = new BackendCastleService(sl);
        }
    }
}
