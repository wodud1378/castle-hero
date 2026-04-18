using CastleHero.Network.Service.Boot;

namespace CastleHero.Network.Impl.Local.Boot
{
    public class LocalBootServiceFactory : IBootServiceFactory
    {
        public IBootService Create(BootConfig config, IBootServiceHandler handler)
            => new LocalBootService(handler);
    }
}
