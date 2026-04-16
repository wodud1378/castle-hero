using Cysharp.Threading.Tasks;

namespace CastleHero.Network.Service.Boot
{
    /// <summary>
    /// 앱 부팅 흐름 (로그인 → 데이터 로드 → 초기화 완료) 를 추상화.
    /// 구현체는 플랫폼별로 다름 (Impl.Backend 의 BackendBootService 등).
    /// </summary>
    public interface IBootService
    {
        UniTask Start();
    }
}
