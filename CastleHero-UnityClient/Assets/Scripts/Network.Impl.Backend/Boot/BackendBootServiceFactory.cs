using CastleHero.Network.Service.Boot;

namespace CastleHero.Network.Impl.Backend.Boot
{
    public class BackendBootServiceFactory : IBootServiceFactory
    {
        public IBootService Create(BootConfig config, IBootServiceHandler handler)
            => new BackendBootService(config, handler);
    }
}
