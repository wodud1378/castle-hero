using Cysharp.Threading.Tasks;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Login;

namespace CastleHero.Network.Impl.Local.Boot
{
    public class LocalLoginService : ILoginService
    {
        public UniTask<Result> Login() => UniTask.FromResult(Result.Complete());
    }
}
