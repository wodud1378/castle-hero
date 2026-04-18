using BackEnd;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Login;

namespace CastleHero.Network.Impl.Backend.Login
{
    public class BackendAutoLoginService : BackendNetworkServiceBase, ILoginService
    {
        public BackendAutoLoginService(IServiceLocator sl) : base(sl) { }

        public async UniTask<Result> Login()
            => await Call(global::BackEnd.Backend.BMember.LoginWithTheBackendToken);
    }
}
