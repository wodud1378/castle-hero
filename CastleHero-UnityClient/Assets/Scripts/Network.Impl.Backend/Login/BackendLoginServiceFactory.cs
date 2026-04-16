using CastleHero.Network.Service.Login;

namespace CastleHero.Network.Impl.Backend.Login
{
    public class BackendLoginServiceFactory : ILoginServiceFactory
    {
        public ILoginService Create(Platform platform) => platform switch
        {
            Platform.PlayStore => new BackendGPGSLoginService(),
            Platform.Guest => new BackendGuestLoginService(),
            _ => null,
        };
    }
}
