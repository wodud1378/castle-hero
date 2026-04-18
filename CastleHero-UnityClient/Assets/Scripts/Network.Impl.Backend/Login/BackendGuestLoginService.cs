using BackEnd;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Login;

namespace CastleHero.Network.Impl.Backend.Login
{
    public class BackendGuestLoginService : BackendNetworkServiceBase, ILoginService
    {
        public BackendGuestLoginService(IServiceLocator sl) : base(sl) { }

        public async UniTask<Result> Login()
        {
            BackendResult result = null;
            for (int i = 0; i < 3 && result is not { IsSuccess: true }; ++i)
            {
                result = await Call(global::BackEnd.Backend.BMember.GuestLogin);
            }

            return result;
        }
    }
}
