using Cysharp.Threading.Tasks;
using CastleHero.Network.Shared;

namespace CastleHero.Network.Service
{
    public interface ISummonService
    {
        UniTask<Result<Summon>> SummonOnce(int eventId, int costIndex);
        UniTask<Result<Summon>> SummonTenth(int eventId, int costIndex);
    }
}
