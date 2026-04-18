using CastleHero.Network.Service.Login;

namespace CastleHero.Network.Impl.Local.Boot
{
    public class LocalLoginServiceFactory : ILoginServiceFactory
    {
        public ILoginService Create(Platform platform) => new LocalLoginService();
    }
}
