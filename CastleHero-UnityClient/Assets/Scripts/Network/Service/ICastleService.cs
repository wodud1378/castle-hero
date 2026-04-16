using Cysharp.Threading.Tasks;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Service
{
    public interface ICastleService
    {
        UniTask<Result<CastleGrowth>> LvUp();
    }
}
