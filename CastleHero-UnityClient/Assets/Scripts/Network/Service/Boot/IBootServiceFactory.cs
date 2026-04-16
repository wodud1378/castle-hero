namespace CastleHero.Network.Service.Boot
{
    /// <summary>
    /// IBootService 인스턴스 생성 팩토리. View (TitleBehaviour) 가 구현체를 직접 참조하지 않도록 추상화.
    /// </summary>
    public interface IBootServiceFactory
    {
        IBootService Create(BootConfig config, IBootServiceHandler handler);
    }
}
