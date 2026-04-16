namespace CastleHero.Network.Service
{
    public interface INetworkServiceProvider
    {
        IUserService User { get; }
        IGameService Game { get; }
        ICharacterService Character { get; }
        IInventoryService Inventory { get; }
        IShopService Shop { get; }
        ISummonService Summon { get; }
        ICastleService Castle { get; }
    }
}
