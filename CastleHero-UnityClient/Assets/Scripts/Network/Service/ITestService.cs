using Cysharp.Threading.Tasks;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Service
{
    /// <summary>
    /// 에디터/QA 용 테스트 서비스. 프로덕션 빌드에서는 의미 없으나 인터페이스를 통해 View 의 직참조를 제거.
    /// </summary>
    public interface ITestService
    {
        UniTask<Result> AddItems(int[] ids, int[] quantities);
        UniTask<Result> AddCurrency(CurrencyDto add);
    }
}
