using CastleHero.Common.Pattern;
using CastleHero.Network.Service.Login;

namespace CastleHero.Network.Impl.Backend.Login
{
    public class BackendLoginServiceFactory : ILoginServiceFactory
    {
        private readonly IServiceLocator _sl = ServiceLocator.Instance;

        public ILoginService Create(Platform platform) => platform switch
        {
            Platform.PlayStore => new BackendGPGSLoginService(_sl),
            Platform.Guest => new BackendGuestLoginService(_sl),
            _ => null,
        };
    }
}
