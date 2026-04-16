namespace CastleHero.Network.Service.Login
{
    /// <summary>
    /// 플랫폼별 로그인 서비스를 제공하는 팩토리. View 가 구현체를 직접 참조하지 않도록 추상화.
    /// </summary>
    public interface ILoginServiceFactory
    {
        ILoginService Create(Platform platform);
    }
}
